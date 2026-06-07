using System.Security.Claims;
using System.Text.RegularExpressions;
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
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    IJwtService jwtTokenService,
    AppDbContext context,
    IRefreshTokenRepository refreshTokenRepository,
    IHttpContextAccessor httpContextAccessor,
    IOptions<GoogleAuthSettings> googleAuthSettings,
    IEmailService emailService,
    IOtpService otpService) : IAuthService
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
                Message =
                    $"Failed to create user: {string.Join(", ", creationResult.Errors.Select(e => e.Description))}"
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
            await context.Patients.AddAsync(new Patient { UserId = newUser.Id });
        }

        var roleResult = await userManager.AddToRoleAsync(newUser, role);
        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync();
            return new Result { Success = false, Message = "Failed to assign role to new user" };
        }

        var emailRowExists = await context.UserEmails
            .AnyAsync(e => e.Email == registerDto.Email.ToLowerInvariant());

        if (!emailRowExists)
        {
            await context.UserEmails.AddAsync(new UserEmail
            {
                UserId = newUser.Id,
                Email = registerDto.Email.ToLowerInvariant(),
                IsPrimary = true,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        string message;
        if (registerDto.IsDoctor)
        {
            if (!string.IsNullOrWhiteSpace(registerDto.Email))
            {
                var html = $"""
                            <div style="font-family:sans-serif;max-width:520px;margin:auto">
                              <h2 style="color:#f59e0b">Registration Under Review – Clinical</h2>
                              <p>Hi <strong>{registerDto.Username ?? "Doctor"}</strong>,</p>
                              <p>Your doctor registration has been received and is currently <strong>under review</strong>. We'll notify you once a decision has been made.</p>
                              <hr style="border:none;border-top:1px solid #e5e7eb;margin:24px 0"/>
                              <p style="color:#6b7280;font-size:13px">If you have any questions, please reach out to our support team.</p>
                            </div>
                            """;
                _ = emailService.SendAsync(registerDto.Email, "Your Clinical registration is under review", html);
            }

            message = AuthConstants.Messages.RegisterationSubmitted;
        }
        else
        {
            message = AuthConstants.Messages.UserRegisteredSuccessfully;
        }

        return new Result { Success = true, Message = message };
    }

    public async Task<AuthResult> LoginServiceAsync(LoginDto loginDto)
    {
        AppUser? user = loginDto.UsernameOrEmail.Contains('@')
            ? await userManager.FindByEmailAsync(loginDto.UsernameOrEmail)
            : await userManager.FindByNameAsync(loginDto.UsernameOrEmail);

        if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password) || user.IsDeleted)
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
        {
            return new AuthResult
            {
                Success = false,
                Message = "User not found"
            };
        }

        if (user.Email == updateEmail.NewEmail)
        {
            return new AuthResult
            {
                Success = false,
                Message = "The email you entered has not changed"
            };
        }


        user.Email = updateEmail.NewEmail;
        var updateResult = await userManager.UpdateAsync(user);
        return updateResult.Succeeded
            ? new AuthResult { Success = true }
            : new AuthResult
                { Success = false, Message = string.Join(", ", updateResult.Errors.Select(e => e.Description)) };
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
            : new AuthResult
                { Success = false, Message = string.Join(", ", updateResult.Errors.Select(e => e.Description)) };
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
            await refreshTokenRepository.RevokeAllUserTokensAsync(token.UserId, RevokeConstants.Messages.OldTokenUsage);
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

    public async Task<GoogleLoginResult> GoogleLoginServiceAsync(GoogleLoginDto dto)
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
            return new GoogleLoginResult { Message = AuthConstants.Messages.GoogleAuthFailed };
        }

        var email = payload.Email;

        // 2. Find existing user by email
        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            if (existingUser.IsDeleted)
                return new GoogleLoginResult { Message = "User is deleted" };

            var roles = await userManager.GetRolesAsync(existingUser);
            if (roles.Contains(Role.Doctor.ToString()))
            {
                var status = await GetDoctorApprovalStatusAsync(existingUser);
                if (status == DoctorApprovalStatus.Pending)
                    return new GoogleLoginResult { Success = false, Message = AuthConstants.Messages.StatusPending };
                if (status == DoctorApprovalStatus.Rejected)
                    return new GoogleLoginResult { Success = false, Message = AuthConstants.Messages.StatusRejected };
            }

            var authResult = await BuildAuthResultAsync(existingUser);
            return new GoogleLoginResult
            {
                Success = authResult.Success,
                Message = authResult.Message,
                AccessToken = authResult.AccessToken,
                AccessTokenExpiration = authResult.AccessTokenExpiration,
                RefreshToken = authResult.RefreshToken,
                RefreshTokenExpiration = authResult.RefreshTokenExpiration
            };
        }

        // 3. New user — if they want to be a Doctor we can't complete registration yet
        if (dto.IsDoctor)
        {
            return new GoogleLoginResult
            {
                Success = false,
                RequiresRegistration = true,
                Message =
                    "Doctor registration requires additional information. Please provide your Professional Practice License and Issuing Authority."
            };
        }

        // 4. New Patient — auto-register and log in immediately
        var username = await GenerateUniqueUsernameAsync(payload.Name ?? email.Split('@')[0]);

        var newUser = new AppUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var creationResult = await userManager.CreateAsync(newUser);
            if (!creationResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return new GoogleLoginResult
                {
                    Message =
                        $"Failed to create user: {string.Join(", ", creationResult.Errors.Select(e => e.Description))}"
                };
            }

            await context.Patients.AddAsync(new Patient { UserId = newUser.Id });

            var roleResult = await userManager.AddToRoleAsync(newUser, "Patient");
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return new GoogleLoginResult { Message = "Failed to assign role to new user" };
            }

            await context.UserEmails.AddAsync(new UserEmail
            {
                UserId = newUser.Id,
                Email = email.ToLowerInvariant(),
                IsPrimary = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            var authResult = await BuildAuthResultAsync(newUser);
            return new GoogleLoginResult
            {
                Success = authResult.Success,
                Message = authResult.Message,
                AccessToken = authResult.AccessToken,
                AccessTokenExpiration = authResult.AccessTokenExpiration,
                RefreshToken = authResult.RefreshToken,
                RefreshTokenExpiration = authResult.RefreshTokenExpiration
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new GoogleLoginResult { Message = ex.Message };
        }
    }

    /// <summary>
    /// Completes Google Doctor registration after the client has collected license fields.
    /// The same id_token from the initial /google-login call is re-validated here.
    /// </summary>
    public async Task<AuthResult> GoogleRegisterDoctorAsync(GoogleRegisterDoctorDto dto)
    {
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

        // 2. Guard: account must not already exist
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
            return new AuthResult
            {
                Success = false,
                Message = "An account with this Google address already exists. Please use the login endpoint."
            };

        // Guard 2: check UserEmails table for orphaned rows from previous partial failures.
        // When MARS is enabled, SaveChangesAsync() calls inside a transaction can commit
        // individually even if a later step fails and the transaction is rolled back.
        // This leaves ghost rows that would cause a duplicate-key crash on the next attempt.
        var emailTaken = await context.UserEmails.AnyAsync(e => e.Email == email.ToLowerInvariant());
        if (emailTaken)
            return new AuthResult
            {
                Success = false,
                Message = "An account with this email already exists. Please use the login endpoint or contact support."
            };

        var username = await GenerateUniqueUsernameAsync(payload.Name ?? email.Split('@')[0]);

        var newUser = new AppUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var creationResult = await userManager.CreateAsync(newUser);
            if (!creationResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return new AuthResult
                {
                    Message =
                        $"Failed to create user: {string.Join(", ", creationResult.Errors.Select(e => e.Description))}"
                };
            }

            await context.Doctors.AddAsync(new Doctor
            {
                UserId = newUser.Id,
                ProfessionalPracticeLicense = dto.ProfessionalPracticeLicense,
                IssuingAuthority = dto.IssuingAuthority
            });

            var roleResult = await userManager.AddToRoleAsync(newUser, "Doctor");
            if (!roleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return new AuthResult { Message = "Failed to assign role to new user" };
            }

            await context.UserEmails.AddAsync(new UserEmail
            {
                UserId = newUser.Id,
                Email = email.ToLowerInvariant(),
                IsPrimary = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Send "registration under review" email
            var html = $"""
                        <div style="font-family:sans-serif;max-width:520px;margin:auto">
                          <h2 style="color:#f59e0b">Registration Under Review – Clinical</h2>
                          <p>Hi <strong>{username}</strong>,</p>
                          <p>Your doctor registration has been received and is currently <strong>under review</strong>. We'll notify you once a decision has been made.</p>
                          <hr style="border:none;border-top:1px solid #e5e7eb;margin:24px 0"/>
                          <p style="color:#6b7280;font-size:13px">If you have any questions, please reach out to our support team.</p>
                        </div>
                        """;
            _ = emailService.SendAsync(email, "Your Clinical registration is under review", html);

            return new AuthResult { Success = true, Message = AuthConstants.Messages.RegisterationSubmitted };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new AuthResult { Message = ex.Message };
        }
    }

    public async Task<Result> RequestDeleteAccountOtpAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null || user.IsDeleted)
            return Fail(AuthConstants.Messages.UserNotFound);

        if (!user.IsGoogleUser)
            return Fail("This endpoint is only for Google-authenticated accounts. Use your password to delete your account.");

        var primaryEmail = await GetPrimaryEmailAsync(user);
        if (primaryEmail is null)
            return Fail("No verified email on file. Please contact support to delete your account.");

        await otpService.IssueAsync(
            user.Id,
            user.UserName ?? user.Email!,
            primaryEmail,
            OtpPurpose.AccountDeletion);

        return Ok($"A deletion confirmation code has been sent to {MaskEmail(primaryEmail)}. It expires in 10 minutes.");
    }

    public async Task<Result> DeleteAccountService(string userId, string? userRole, DeleteAccountDto dto)
    {
        if (userRole is not null && userRole.IsAdmin())
            return Fail("Can't delete this account");

        var user = await userManager.FindByIdAsync(userId);
        if (user is null || user.IsDeleted)
            return Fail(AuthConstants.Messages.UserNotFound);

        if (user.IsGoogleUser)
        {
            if (string.IsNullOrWhiteSpace(dto.OtpCode))
                return Fail("Your account uses Google sign-in. Please request a deletion code via POST /api/auth/delete/otp and provide it in the 'otpCode' field.");

            var record = await otpService.VerifyAsync(user.Id, OtpPurpose.AccountDeletion, dto.OtpCode);
            if (record is null)
                return Fail("Invalid or expired verification code. Request a new one via POST /api/auth/delete/otp.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.Password))
                return Fail("Password is required to delete your account.");

            if (!await userManager.CheckPasswordAsync(user, dto.Password))
                return Fail(AuthConstants.Messages.InvalidCredentials);
        }

        return await PerformDeletionAsync(user, userRole);
    }

    private async Task<Result> PerformDeletionAsync(AppUser user, string? userRole)
    {
        user.IsDeleted = true;
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Fail(string.Join(", ", updateResult.Errors.Select(e => e.Description)));

        await refreshTokenRepository.RevokeAllUserTokensAsync(user.Id, RevokeConstants.Messages.UserDeleted);

        if (userRole is not null && userRole.IsDoctor())
        {
            var doctor = await context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor is not null) doctor.IsDeleted = true;
        }
        else
        {
            var patient = await context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (patient is not null) patient.IsDeleted = true;
        }

        await context.SaveChangesAsync();
        return Ok("Account deleted successfully");
    }

    private async Task<string?> GetPrimaryEmailAsync(AppUser user)
    {
        return await context.UserEmails
            .Where(e => e.UserId == user.Id && e.IsPrimary && e.IsVerified)
            .Select(e => e.Email)
            .FirstOrDefaultAsync()
            ?? user.Email;
    }

    private static Result Ok(string message) => new() { Success = true, Message = message };
    private static Result Fail(string message) => new() { Success = false, Message = message };

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return email;
        return email[0] + new string('*', Math.Min(at - 1, 4)) + email[at..];
    }

    private async Task<Result> CheckExistence(string username, string email)
    {
        var userExists = await context.Users
            .Where(u => u.UserName == username || u.Email == email)
            .Select(u => new { u.UserName, u.Email })
            .FirstOrDefaultAsync();

        if (userExists is not null)
        {
            var message = userExists.UserName == username
                ? AuthConstants.Messages.UsernameAlreadyExists
                : AuthConstants.Messages.EmailAlreadyExists;
            return new Result { Success = false, Message = message };
        }

        // Also check UserEmails table — guards against orphaned rows left by prior partial failures
        var emailRowExists = await context.UserEmails.AnyAsync(e => e.Email == email.ToLowerInvariant());
        if (emailRowExists)
            return new Result { Success = false, Message = AuthConstants.Messages.EmailAlreadyExists };

        return new Result { Success = true };
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

        return new Result<AppUser>
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
            new(ClaimTypes.Email, user.Email!),
            new("auth_provider", user.IsGoogleUser ? "google" : "local")
        };
        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));
        var userClaims = await userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);
        return claims;
    }

    private async Task<AuthResult> BuildAuthResultAsync(AppUser user)
    {
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

    private async Task<string> GenerateUniqueUsernameAsync(string displayName)
    {
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