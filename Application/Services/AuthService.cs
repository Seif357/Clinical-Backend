using System.Security.Claims;
using System.Text.RegularExpressions;
using Application.Dto;
using Application.Dto.AuthDto;
using Application.DTOs;
using Application.ExtentionMethods;
using Application.Interfaces;
using Application.Mapper;
using Domain.Constants;
using Domain.Models;
using Domain.Models.Auth;
using Google.Apis.Auth;
using Infrastructure.Configurations;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
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
    IHttpContextAccessor httpContextAccessor,
    IOptions<GoogleAuthSettings> googleAuthSettings) : IAuthService

{
    public async Task<Result> RegisterServiceAsync(RegisterDto registerDto)
    {
        var result = await CheckExistence(registerDto.Username, registerDto.Email);
        if (!result.Success) return result;
        if (registerDto.Password != registerDto.ConfirmPassword)
        {
            return new Result
            {
                Success = false,
                Message = "Passwords don't match"
            };
        }

        var newUser = registerDto.ToEntity();
        await using var transaction = await context.Database.BeginTransactionAsync();
        
            var creationResult = await userManager.CreateAsync(newUser, registerDto.Password);
            if (!creationResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return new Result
                {
                    Success = false,
                    Message = $"Failed to create user: {string.Join(", ", creationResult.Errors.Select(e => e.Description))}"
                };
            }

            string role;
            if (registerDto.IsDoctor)
            {
                if (string.IsNullOrWhiteSpace(registerDto.ProfessionalPracticeLicense) ||
                    string.IsNullOrWhiteSpace(registerDto.IssuingAuthority))
                {
                    await transaction.RollbackAsync();
                    return new Result
                    {
                        Success = false,
                        Message = "Professional Practice License and Issuing Authority are required for doctor registration"
                    };
                }

                role = "Doctor";
                await context.Doctors.AddAsync(new Doctor
                {
                    UserId = newUser.Id,
                    ProfessionalPracticeLicense = registerDto.ProfessionalPracticeLicense,
                    IssuingAuthority = registerDto.IssuingAuthority
                });
            }
            else
            {
                role = "Patient";
                await context.Patients.AddAsync(new Patient
                {
                    UserId = newUser.Id
                });
            }
            
            var roleResult = await userManager.AddToRoleAsync(newUser, role);
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return new Result { Success = false, Message = "Failed to assign role to new user" };
            }
 
            // Seed primary email row
            await context.UserEmails.AddAsync(new UserEmail
            {
                UserId     = newUser.Id,
                Email      = registerDto.Email.ToLowerInvariant(),
                IsPrimary  = true,
                IsVerified = false,
                CreatedAt  = DateTime.UtcNow
            });
 
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
 
            return new Result { Success = true, Message = AuthConstants.Messages.UserRegisteredSuccessfully };
    }

    public async Task<AuthResult> LoginServiceAsync(LoginDto loginDto)
    {
        AppUser? user = loginDto.UsernameOrEmail.Contains('@')
            ? await userManager.FindByEmailAsync(loginDto.UsernameOrEmail)
            : await userManager.FindByNameAsync(loginDto.UsernameOrEmail);

            if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password)||user.IsDeleted)
            {
                return new AuthResult
                {
                    Success = false, 
                    Message = AuthConstants.Messages.InvalidCredentials
                };
            }
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Contains(Role.Doctor.ToString()))
            {
                var status = await GetDoctorApprovalStatusAsync(user);
                if (status.Equals(DoctorApprovalStatus.Pending))
                {
                    return new AuthResult
                    {
                        Success = false, 
                        Message = AuthConstants.Messages.StatusPending
                    };
                }
                if (status.Equals(DoctorApprovalStatus.Rejected))
                {
                    return new AuthResult
                    {
                        Success = false, 
                        Message = AuthConstants.Messages.StatusRejected
                    };
                }
            }
            await context.SaveChangesAsync();
            var claims = await GenerateUserClaimsAsync(user);
            var accessToken = jwtTokenService.GenerateAccessToken(claims);
            var refreshToken = jwtTokenService.GenerateRefreshToken(user.Id);

            await refreshTokenRepository.AddAsync(refreshToken);
            await context.SaveChangesAsync();
            
            return new AuthResult
            {
                Success = true,
                AccessToken = accessToken,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(jwtTokenService.GetTokenExpirationMinutes()),
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(jwtTokenService.GetRefreshTokenExpirationDays())
            };
    }

    public async Task<AuthResult> UpdateEmailServiceAsync(string userId, UpdateEmailDto updateEmail)
    {
        
            var user = await userManager.FindByIdAsync(userId);
            if (user is null || user.IsDeleted)
            {  return new AuthResult
                {
                    Success = false, 
                    Message = "User not found"
                };
            }
            if (user.Email == updateEmail.NewEmail)
            {   return new AuthResult
                {
                    Success = false, 
                    Message = "The email you entered has not changed"
                };
            }


            user.Email = updateEmail.NewEmail;
            var updateResult = await userManager.UpdateAsync(user);
            return updateResult.Succeeded
                ? new AuthResult { Success = true }
                : new AuthResult { Success = false, Message = string.Join(", ", updateResult.Errors.Select(e => e.Description)) };
    }

    public async Task<AuthResult> UpdateUsernameServiceAsync(string userId, UpdateUsernameDto updateUsernameDto)
    {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null || user.IsDeleted)
            {
                return new AuthResult
                {
                    Success = false, 
                    Message = "User not found"
                };
            }

            if (user.UserName == updateUsernameDto.NewUserName)
            {
                return new AuthResult
                {
                    Success = false, 
                    Message = "The username you entered has not changed"
                };
            }
            
            user.UserName = updateUsernameDto.NewUserName;
            var updateResult = await userManager.UpdateAsync(user);
            return updateResult.Succeeded
                ? new AuthResult { Success = true }
                : new AuthResult { Success = false, Message = string.Join(", ", updateResult.Errors.Select(e => e.Description)) };
    }
    
    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
            if (string.IsNullOrEmpty(refreshToken)) 
                throw new SecurityTokenException("Refresh Token is required");
            var token = await refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token == null) 
                throw new SecurityTokenException("Invalid Refresh Token");

            if (!token.IsActive)
            {
                await refreshTokenRepository.RevokeAllUserTokensAsync(token.UserId,RevokeConstants.Messages.OldTokenUsage);
                await context.SaveChangesAsync();
                throw new SecurityTokenException("Refresh token reuse detected — all sessions have been revoked");

            } 
            await RevokeTokenServiceAsync(refreshToken, RevokeConstants.Messages.RefreshTokenReplaced);
                var user = await userManager.FindByIdAsync(token.UserId.ToString());
                if (user == null)
                    throw new SecurityTokenException("User is not found");
                var newRefreshToken = jwtTokenService.GenerateRefreshToken(user.Id);
                newRefreshToken.DeviceId = token.DeviceId; // carry device forward
                await refreshTokenRepository.AddAsync(newRefreshToken);
                await context.SaveChangesAsync();
                
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

    public async Task<bool> RevokeTokenServiceAsync(string refreshToken, string? revokeReason = null)
    {
        if (string.IsNullOrEmpty(refreshToken)) 
            throw new SecurityTokenException("Refresh Token is required");
        var token = await refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (token == null)
            throw new SecurityTokenException("Invalid Refresh Token");
        if (!token.IsActive)
            throw new SecurityTokenException("Deactivated Refresh Token");

        token.RevokedAt = DateTime.UtcNow;
        token.ReasonRevoked = revokeReason;
        await refreshTokenRepository.UpdateAsync(token);
        return await context.SaveChangesAsync() >= 1;
    }

    public async Task<Result> DeleteAccountService(string userId, string userRole, string password)
    {
        if (userRole.IsAdmin())
        {
            return new Result
            {
                Success = false,
                Message = "Can't delete this account"
            };
        }
        var verificationResult = await VerifyPasswordAsync(userId, password);
        if (!verificationResult.Success)
            return new Result { Success = false, Message = verificationResult.Message };
 
        var user = verificationResult.Data!;
        user.IsDeleted = true;
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return new Result { Success = false, Message = string.Join(", ", updateResult.Errors.Select(e => e.Description)) };
 
        await refreshTokenRepository.RevokeAllUserTokensAsync(user.Id, RevokeConstants.Messages.UserDeleted);
 
        if (userRole.IsDoctor())
        {
            var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor != null) doctor.IsDeleted = true;
        }
        else if (userRole.IsPatient())
        {
            var patient = await context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (patient != null) patient.IsDeleted = true;
        }
 
        await context.SaveChangesAsync();
        return new Result { Success = true, Message = "Account deleted successfully" };
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
    private async Task<Result> CheckExistence(string username, string email)
    {
        var userExists = await context.Users
            .Where(u => u.UserName == username || u.Email == email)
            .Select(u => new { u.Id, u.UserName, u.Email})
            .FirstOrDefaultAsync();

        if (userExists is null)
        {
            return new Result
            {
                Success = true
            };
        }

        var message = userExists.UserName == username
            ? AuthConstants.Messages.UsernameAlreadyExists
            : AuthConstants.Messages.EmailAlreadyExists;

        return new Result
        {
            Success = false, 
            Message = message
        };
    }
    private async Task<Result<AppUser>> VerifyPasswordAsync(string userId, string password)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user == null || !await userManager.CheckPasswordAsync(user, password) || user.IsDeleted)
        {
            return new Result<AppUser>
            {
                Success = false,
                Message = AuthConstants.Messages.InvalidCredentials,
            };
        }
        return  new Result<AppUser>
        {
            Success = true,
            Data = user
        };
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
    private async Task<DoctorApprovalStatus?> GetDoctorApprovalStatusAsync(AppUser user)
    {
        return await context.Doctors
            .Where(p => p.UserId == user.Id)
            .Select(s => (DoctorApprovalStatus?)s.ApprovalStatus)
            .FirstOrDefaultAsync();
    }
}