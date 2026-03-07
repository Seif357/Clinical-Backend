using System.Security.Claims;
using Application.Dto.AuthDto;
using Application.DTOs;
using Application.ExtentionMethods;
using Application.Interfaces;
using Application.Mapper;
using Domain.Constants;
using Domain.Models;
using Domain.Models.Auth;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    IJwtService jwtTokenService,
    AppDbContext context,
    IRefreshTokenRepository refreshTokenRepository) : IAuthService

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
            if (roleResult.Succeeded)
            {
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return new Result
                {
                    Success = true, 
                    Message = AuthConstants.Messages.UserRegisteredSuccessfully
                };
            }
            else
            {
                await transaction.RollbackAsync();
                return new Result
                {
                    Success = false, 
                    Message = "Failed to assign role to new user"
                };

            }
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
            if (updateResult.Succeeded)
            {
                return new AuthResult { Success = true };

            }
            else
            {
                return new AuthResult
                {
                    Success = false,
                    Message = $"Failed to update email: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}"
                };
            }
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
            if (updateResult.Succeeded)
            {
                return new AuthResult
                {
                    Success = true
                };
            }
            else
            {
                return new AuthResult
                {
                    Success = false,
                    Message = $"Failed to update username: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}"
                };
            }
    }

    /// <summary>
    /// Verifies the user's password and returns the AppUser on success via Result.Data,
    /// avoiding a redundant FindByIdAsync call in the caller.
    /// </summary>
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

    public async Task<AuthResult> UpdatePasswordServiceAsync(string userId, UpdatePasswordDto updatePassDto)
    {
        if (updatePassDto.NewPassword != updatePassDto.ConfirmNewPassword)
        {
            return new AuthResult
            {
                Success = false, 
                Message = "Passwords don't match"
            };
        }

        var verificationResult = await VerifyPasswordAsync(userId, updatePassDto.Password);
        if (verificationResult.Success)
        {
            var user = verificationResult.Data as AppUser;
            var passMatchResult = await userManager.CheckPasswordAsync(user, updatePassDto.NewPassword);
            if (passMatchResult)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "New password must be different from the current password"
                };
            }

            var updateResult = await userManager.ChangePasswordAsync(user, updatePassDto.Password, updatePassDto.NewPassword);
            if (updateResult.Succeeded)
            {
                return new AuthResult { Success = true };
            }
            else
            {
                return new AuthResult
                {
                    Success = false,
                    Message = $"Failed to update password: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}"
                };
            }
        }
        else
        {
            return new AuthResult
            {
                Success = verificationResult.Success,
                Message = verificationResult.Message
            };
        }



            
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
            if (string.IsNullOrEmpty(refreshToken)) 
                throw new SecurityTokenException("Refresh Token is required");
            var token = await refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token == null) 
                throw new SecurityTokenException("Invalid Refresh Token");

            if (token.IsActive)
            {
                await RevokeTokenServiceAsync(refreshToken, RevokeConstants.Messages.RefreshTokenReplaced);
                var user = await userManager.FindByIdAsync(token.UserId.ToString());
                if (user == null)
                    throw new SecurityTokenException("User is not found");

                var newRefreshToken = jwtTokenService.GenerateRefreshToken(user.Id);
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
            else
            {
                await refreshTokenRepository.RevokeAllUserTokensAsync(token.UserId,RevokeConstants.Messages.OldTokenUsage);
                await context.SaveChangesAsync();
                throw new SecurityTokenException("Refresh token reuse detected — all sessions have been revoked");
            }
    }

    public async Task<bool> RevokeTokenServiceAsync(string refreshToken, string? revokeReason = null)
    {
        if (string.IsNullOrEmpty(refreshToken)) 
            throw new SecurityTokenException("didn't find a Refresh Token");
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
        if (verificationResult.Success)
        {
            
            var user = verificationResult.Data;
            user.IsDeleted = true;
            var updateResult = await userManager.UpdateAsync(user);
            if (updateResult.Succeeded)
            {
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
                
                return new Result
                {
                    Success = true,
                    Message = "Account deleted successfully"
                };
            }
            else
            {
                return new Result
                {
                    Success = false,
                    Message =
                        $"Failed to delete account: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}"
                };
            }
        }
        else
        {
            return new Result
            {
                Success = verificationResult.Success,
                Message = verificationResult.Message
            };
        }
    }
}