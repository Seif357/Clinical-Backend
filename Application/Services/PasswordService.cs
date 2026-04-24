using Application.Dto.AuthDto;
using Application.DTOs;
using Application.Interfaces;
using Domain.Models.Auth;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OtpPurpose = Domain.Models.Auth.OtpPurpose;

namespace Application.Services;

public class PasswordService(
    UserManager<AppUser> userManager,
    IOtpService otpService,
    IRefreshTokenRepository refreshTokenRepository,
    AppDbContext context) : IPasswordService
{
    // ── Authenticated: request OTP to change password ─────────────────────────
    public async Task<Result> RequestPasswordChangeOtpAsync(int userId, RequestPasswordChangeOtpDto dto)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted)
            return Fail("User not found.");

        // Verify they know their current password
        if (!await userManager.CheckPasswordAsync(user, dto.CurrentPassword))
            return Fail("Current password is incorrect.");

        // Get primary email to send OTP to
        var primaryEmail = await context.UserEmails
            .Where(e => e.UserId == userId && e.IsPrimary && e.IsVerified)
            .Select(e => e.Email)
            .FirstOrDefaultAsync()
            ?? user.Email;

        if (primaryEmail is null)
            return Fail("No verified email on file. Please add a verified email first.");

        // Hash the new password is NOT done here — we don't know it yet.
        // The user will provide it at confirmation time.
        await otpService.IssueAsync(
            userId,
            user.UserName ?? user.Email!,
            primaryEmail,
            OtpPurpose.PasswordChangeConfirmation);

        return Ok($"A verification code has been sent to {MaskEmail(primaryEmail)}. It expires in 10 minutes.");
    }

    // ── Authenticated: confirm OTP + new password ─────────────────────────────
    public async Task<Result> ConfirmPasswordChangeAsync(int userId, ConfirmPasswordChangeDto dto)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
            return Fail("Passwords do not match.");

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted)
            return Fail("User not found.");

        var record = await otpService.VerifyAsync(userId, OtpPurpose.PasswordChangeConfirmation, dto.OtpCode);
        if (record is null)
            return Fail("Invalid or expired code. Please request a new one.");

        // Generate a reset token and apply new password
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);
        if (!result.Succeeded)
            return Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

        // Revoke all sessions for security
        await refreshTokenRepository.RevokeAllUserTokensAsync(userId, "Password changed by user");
        await context.SaveChangesAsync();

        return Ok("Password updated successfully. All other sessions have been signed out.");
    }

    // ── Unauthenticated: request forgot-password OTP ──────────────────────────
    public async Task<Result> ForgotPasswordRequestAsync(ForgotPasswordRequestDto dto)
    {
        // Always return success to prevent user enumeration
        var contact = dto.EmailOrPhone.Trim();

        AppUser? user = null;
        string? target = null;

        if (contact.Contains('@'))
        {
            user = await userManager.FindByEmailAsync(contact);
            // Also check UserEmails table (secondary emails)
            if (user is null)
            {
                var userEmail = await context.UserEmails
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.Email == contact && e.IsVerified);
                user = userEmail?.User;
            }
            target = contact;
        }
        else
        {
            var phone = await context.UserPhones
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PhoneNumber == contact && p.IsVerified);
            user = phone?.User;
            target = contact;
        }

        if (user is not null && !user.IsDeleted && target is not null)
        {
            await otpService.IssueAsync(
                user.Id,
                user.UserName ?? user.Email!,
                target,
                OtpPurpose.ForgotPassword);
        }

        // Always the same message
        return Ok("If that account exists, a reset code has been sent.");
    }

    // ── Unauthenticated: verify OTP + set new password ────────────────────────
    public async Task<Result> ForgotPasswordResetAsync(ForgotPasswordResetDto dto)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
            return Fail("Passwords do not match.");

        var contact = dto.EmailOrPhone.Trim();

        // Resolve the user from the contact
        AppUser? user = contact.Contains('@')
            ? await userManager.FindByEmailAsync(contact)
            : null;

        if (user is null)
        {
            var phone = await context.UserPhones
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PhoneNumber == contact && p.IsVerified);
            user = phone?.User;
        }

        if (user is null || user.IsDeleted)
            return Fail("Invalid or expired code.");

        var record = await otpService.VerifyAsync(user.Id, OtpPurpose.ForgotPassword, dto.OtpCode);
        if (record is null)
            return Fail("Invalid or expired code.");

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);
        if (!result.Succeeded)
            return Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

        await refreshTokenRepository.RevokeAllUserTokensAsync(user.Id, "Password reset via forgot-password");
        await context.SaveChangesAsync();

        return Ok("Password reset successfully. Please log in with your new password.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static Result Ok(string message) => new() { Success = true, Message = message };
    private static Result Fail(string message) => new() { Success = false, Message = message };

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return email;
        return email[0] + new string('*', Math.Min(at - 1, 4)) + email[at..];
    }
}