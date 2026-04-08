using System.Security.Claims;
using System.Text.RegularExpressions;
using Application.Dto;
using Application.Dto.AuthDto;
using Application.DTOs;
using Application.Interfaces;
using Application.Mapper;
using Domain.Constants;
using Domain.Models;
using Domain.Models.Auth;
using Google.Apis.Auth;
using Infrastructure.Configurations;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    IJwtService jwtTokenService,
    AppDbContext context,
    IRefreshTokenRepository refreshTokenRepository,
    IOptions<GoogleAuthSettings> googleAuthSettings) : IAuthService

{
    public async Task<Result> RegisterServiceAsync(RegisterDto registerDto)
    {
        var result = await CheckExistence(registerDto.Username, registerDto.Email, registerDto.PhoneNumber);
        if (!result.Success) return result;
        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            result.Success = false;
            result.Message = "Password doesn't match";
            return result;
        }

        var newUser = registerDto.ToEntity();
        using var transaction = await context.Database.BeginTransactionAsync();
        transaction.GetDbTransaction();
        try
        {
            var creationResult = await userManager.CreateAsync(newUser, registerDto.Password);
            if (!creationResult.Succeeded)
            {
                transaction.Rollback();
                result.Success = false;
                result.Message =
                    $"Failed to create new user! {string.Join(", ", creationResult.Errors.Select(e => e.Description))}";
                return result;
            }

            string role;
            if (registerDto.IsDoctor == false)
            {
                role = "Patient";
                var patient = new Patient
                {
                    UserId = newUser.Id
                };
                var PatientResult = await context.Patients.AddAsync(patient);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(registerDto.ProfessionalPracticeLicense) ||
                    string.IsNullOrWhiteSpace(registerDto.IssuingAuthority))
                {
                    result.Success = false;
                    result.Message =
                        "Professional Practice License and Issuing Authority are required for doctor registeration";
                    return result;
                }

                role = "Doctor";
                var doctor = new Doctor
                {
                    UserId = newUser.Id,
                    ProfessionalPracticeLicense = registerDto.ProfessionalPracticeLicense,
                    IssuingAuthority = registerDto.IssuingAuthority
                };
                var DoctorResult = await context.Doctors.AddAsync(doctor);
            }

            await context.SaveChangesAsync();
            transaction.Commit();

            if (!(await userManager.AddToRoleAsync(newUser, role)).Succeeded)
            {
                transaction.Rollback();
                result.Success = false;
                result.Message = "Failed to assign role to new user!";
                return result;
            }

            result.Message = AuthConstants.Messages.UserRegisteredSuccessfully;
            return result;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            result.Success = false;
            result.Message = ex.Message;
            result.Data = ex.Data;
            return result;
        }
    }

    public async Task<AuthResult> LoginServiceAsync(LoginDto loginDto)
    {
        try
        {
            AppUser? user;
            var result = new AuthResult();
            if (loginDto.UsernameOrEmail.Contains('@'))
                user = await userManager.FindByEmailAsync(loginDto.UsernameOrEmail);
            else
                user = await userManager.FindByNameAsync(loginDto.UsernameOrEmail);

            if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                result.Message = AuthConstants.Messages.InvalidCredentials;
                return result;
            }

            if (user.IsDeleted)
            {
                result.Success = false;
                result.Message = "User is deleted";
                return result;
            }


            var claims = await GenerateUserClaimsAsync(user);
            var accessToken = jwtTokenService.GenerateAccessToken(claims);
            var refreshToken = jwtTokenService.GenerateRefreshToken(user.Id);
            result.Success = true;
            result.AccessToken = accessToken;
            result.AccessTokenExpiration = DateTime.UtcNow.AddMinutes(jwtTokenService.GetTokenExpirationMinutes());
            result.RefreshToken = refreshToken.Token;
            result.RefreshTokenExpiration = DateTime.UtcNow.AddMinutes(jwtTokenService.GetRefreshTokenExpirationDays());
            return result;
        }
        catch (ApplicationException ex)
        {
            return new AuthResult
            {
                Message = ex.Message
            };
        }
        catch (SecurityTokenException ex)
        {
            return new AuthResult
            {
                Message = ex.Message
            };
        }
    }

    public async Task<AuthResult> UpdateEmailServiceAsync(string userId, UpdateEmailDto updateEmail)
    {
        try
        {
            var result = new AuthResult();
            var user = await userManager.FindByIdAsync(userId);
            if (user.Email == updateEmail.NewEmail)
            {
                result.Success = false;
                result.Message = "The Email you entered has not changed";
                return result;
            }

            if (user.IsDeleted)
            {
                result.Success = false;
                result.Message = "User is deleted";
                return result;
            }

            user.Email = updateEmail.NewEmail;
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                result.Success = false;
                result.Message =
                    $"Failed to update email! {string.Join(", ", updateResult.Errors.Select(e => e.Description))}";
                return result;
            }

            result.Success = true;
            return result;
        }
        catch (ApplicationException ex)
        {
            return new AuthResult
            {
                Message = ex.Message
            };
        }
        catch (SecurityTokenException ex)
        {
            return new AuthResult
            {
                Message = ex.Message
            };
        }
    }

    public async Task<AuthResult> UpdateUsernameServiceAsync(string userId, UpdateUsernameDto updateUsernameDto)
    {
        try
        {
            var result = new AuthResult();
            var user = await userManager.FindByIdAsync(userId);
            if (user.UserName == updateUsernameDto.NewUserName)
            {
                result.Success = false;
                result.Message = "The Username you entered has not changed";
                return result;
            }

            if (user.IsDeleted)
            {
                result.Success = false;
                result.Message = "User is deleted";
                return result;
            }

            user.UserName = updateUsernameDto.NewUserName;
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                result.Success = false;
                result.Message =
                    $"Failed to update username! {string.Join(", ", updateResult.Errors.Select(e => e.Description))}";
                return result;
            }

            result.Success = true;
            return result;
        }
        catch (ApplicationException ex)
        {
            return new AuthResult
            {
                Message = ex.Message
            };
        }
        catch (SecurityTokenException ex)
        {
            return new AuthResult
            {
                Message = ex.Message
            };
        }
    }

    public async Task<AuthResult> UpdatePasswordServiceAsync(string userId, UpdatePasswordDto updatePassDto)
    {
        try
        {
            var result = new AuthResult();

            if (updatePassDto.NewPassword != updatePassDto.ConfirmNewPassword)
            {
                result.Success = false;
                result.Message = "Password doesn't match";
                return result;
            }

            var user = await userManager.FindByIdAsync(userId);

            if (user == null || !await userManager.CheckPasswordAsync(user, updatePassDto.Password))
            {
                result.Message = AuthConstants.Messages.InvalidCredentials;
                return result;
            }

            if (user.IsDeleted)
            {
                result.Success = false;
                result.Message = "User is deleted";
                return result;
            }

            if (!await userManager.CheckPasswordAsync(user, updatePassDto.NewPassword))
            {
                result.Success = false;
                result.Message = "The Password you entered has not changed";
                return result;
            }

            var updateResult =
                await userManager.ChangePasswordAsync(user, updatePassDto.Password, updatePassDto.NewPassword);
            if (!updateResult.Succeeded)
            {
                result.Success = false;
                result.Message =
                    $"Failed to update Password! {string.Join(", ", updateResult.Errors.Select(e => e.Description))}";
                return result;
            }

            result.Success = true;
            return result;
        }
        catch (ApplicationException ex)
        {
            return new AuthResult
            {
                Message = ex.Message
            };
        }
        catch (SecurityTokenException ex)
        {
            return new AuthResult
            {
                Message = ex.Message
            };
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            if (string.IsNullOrEmpty(refreshToken)) throw new SecurityTokenException("Refresh Token is required");
            var token = await refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token == null) throw new SecurityTokenException("Invalid Refresh Token");

            if (!token.IsActive) throw new SecurityTokenException("Deactivated Refresh Token");

            await RevokeTokenServiceAsync(refreshToken, "Replaced by a new refresh token");
            var user = await userManager.FindByIdAsync(token.UserId.ToString());
            if (user == null) throw new SecurityTokenException("User is not found");

            var newRefreshToken = jwtTokenService.GenerateRefreshToken(user.Id);
            var claims = await GenerateUserClaimsAsync(user);
            var accessToken = jwtTokenService.GenerateAccessToken(claims);
            return new AuthResult
            {
                Success = true,
                Message = AuthConstants.Messages.TokenRefreshedSuccessfully,
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(jwtTokenService.GetTokenExpirationMinutes()),
                RefreshTokenExpiration = newRefreshToken.ExpiresAt
            };
        }
        catch (ApplicationException ex)
        {
            return new AuthResult
            {
                Message = ex.Message
            };
        }
        catch (SecurityTokenException ex)
        {
            return new AuthResult
            {
                Message = ex.Message
            };
        }
    }

    public async Task<bool> RevokeTokenServiceAsync(string refreshToken, string? revokeReason = null)
    {
        if (string.IsNullOrEmpty(refreshToken)) throw new SecurityTokenException("didn't find a Refresh Token");
        var token = await refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (token == null) throw new SecurityTokenException("Invalid Refresh Token");

        if (!token.IsActive) throw new SecurityTokenException("Deactivated Refresh Token");

        token.RevokedAt = DateTime.UtcNow;
        token.ReasonRevoked = revokeReason;
        await refreshTokenRepository.UpdateAsync(token);
        var success = await context.SaveChangesAsync() >= 1;
        return success;
    }

    private async Task<Result> CheckExistence(string username, string email, string phoneNumber)
    {
        var userExists = await userManager.FindByNameAsync(username);
        var result = new Result();
        if (userExists != null)
        {
            result.Message = AuthConstants.Messages.UsernameAlreadyExists;
            return result;
        }

        userExists = await userManager.FindByEmailAsync(email);
        if (userExists != null)
        {
            result.Message = AuthConstants.Messages.EmailAlreadyExists;
            return result;
        }

        userExists = await context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        if (userExists != null)
        {
            result.Message = AuthConstants.Messages.PhoneAlreadyExists;
            return result;
        }

        result.Success = true;
        return result;
    }

    private async Task<List<Claim>> GenerateUserClaimsAsync(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email!)
        };
        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));
        var userClaims = await userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);
        return claims;
    }

    public async Task<Result> DeleteAccountService(string userId)
    {
        var result = new Result();
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            result.Message = "User not found";
            return result;
        }
        user.IsDeleted = true;
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            result.Success = false;
            result.Message =
                $"Failed to delete account! {string.Join(", ", updateResult.Errors.Select(e => e.Description))}";
            return result;
        }
        result.Success = true;
        result.Message = "Account deleted successfully";
        return result;
    }

    /// <summary>
    /// Validates a Google id_token issued to the client, then finds-or-creates
    /// an AppUser + Patient/Doctor row and returns our own JWT pair.
    /// </summary>
    public async Task<AuthResult> GoogleLoginServiceAsync(GoogleLoginDto dto)
    {
        // 1. Validate the Google id_token server-side
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { googleAuthSettings.Value.ClientId }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
        }
        catch (InvalidJwtException)
        {
            return new AuthResult { Message = AuthConstants.Messages.GoogleAuthFailed };
        }

        var email = payload.Email;

        // 2. Find existing user by email (handles both Google-first and password-first accounts)
        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            // --- Existing user: log them in ---
            if (existingUser.IsDeleted)
                return new AuthResult { Message = "User is deleted" };

            return await BuildAuthResultAsync(existingUser);
        }

        // 3. New user — validate Doctor-specific fields before creating
        if (dto.IsDoctor)
        {
            if (string.IsNullOrWhiteSpace(dto.ProfessionalPracticeLicense) ||
                string.IsNullOrWhiteSpace(dto.IssuingAuthority))
            {
                return new AuthResult
                {
                    Message = "Professional Practice License and Issuing Authority are required for doctor registration"
                };
            }
        }

        // 4. Auto-generate a unique username from the Google display name
        var username = await GenerateUniqueUsernameAsync(payload.Name ?? email.Split('@')[0]);

        var newUser = new AppUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true,  // Google has already verified the email
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Create the identity user without a password
            var creationResult = await userManager.CreateAsync(newUser);
            if (!creationResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return new AuthResult
                {
                    Message = $"Failed to create user: {string.Join(", ", creationResult.Errors.Select(e => e.Description))}"
                };
            }

            string role;
            if (!dto.IsDoctor)
            {
                role = "Patient";
                await context.Patients.AddAsync(new Patient { UserId = newUser.Id });
            }
            else
            {
                role = "Doctor";
                await context.Doctors.AddAsync(new Doctor
                {
                    UserId = newUser.Id,
                    ProfessionalPracticeLicense = dto.ProfessionalPracticeLicense!,
                    IssuingAuthority = dto.IssuingAuthority!
                });
            }

            await context.SaveChangesAsync();

            var roleResult = await userManager.AddToRoleAsync(newUser, role);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return new AuthResult { Message = "Failed to assign role to new user" };
            }

            await transaction.CommitAsync();
            return await BuildAuthResultAsync(newUser);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new AuthResult { Message = ex.Message };
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>Generates JWT + refresh token for an already-verified user.</summary>
    private async Task<AuthResult> BuildAuthResultAsync(AppUser user)
    {
        var claims = await GenerateUserClaimsAsync(user);
        var accessToken = jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = jwtTokenService.GenerateRefreshToken(user.Id);
        return new AuthResult
        {
            Success = true,
            AccessToken = accessToken,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(jwtTokenService.GetTokenExpirationMinutes()),
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiration = DateTime.UtcNow.AddDays(jwtTokenService.GetRefreshTokenExpirationDays())
        };
    }

    /// <summary>
    /// Derives a URL-safe username from a display name and appends a random
    /// numeric suffix until a unique one is found.
    /// </summary>
    private async Task<string> GenerateUniqueUsernameAsync(string displayName)
    {
        // Replace spaces/special chars with dots, lowercase
        var baseUsername = Regex.Replace(displayName.ToLowerInvariant(), @"[^a-z0-9._+\-]", ".");
        baseUsername = Regex.Replace(baseUsername, @"\.{2,}", ".").Trim('.');
        if (string.IsNullOrEmpty(baseUsername)) baseUsername = "user";

        var candidate = baseUsername;
        var random = new Random();
        while (await userManager.FindByNameAsync(candidate) != null)
        {
            candidate = $"{baseUsername}_{random.Next(10, 9999)}";
        }

        return candidate;
    }
}