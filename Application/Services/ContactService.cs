using Application.Dto.AuthDto;
using Application.Dto.Email_management;
using Application.Dto.Phone_management;
using Application.DTOs;
using Application.Interfaces;
using Domain.Models.Auth;
using Infrastructure.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OtpPurpose = Domain.Models.Auth.OtpPurpose;

namespace Application.Services;

public class ContactService(
    UserManager<AppUser> userManager,
    IOtpService otpService,
    AppDbContext context) : IContactService
{
    // ─────────────────────────────────────────────────────────────────────────
    // EMAIL
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<Result> AddEmailAsync(int userId, string displayName, AddEmailDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var exists = await context.UserEmails.AnyAsync(e => e.Email == email)
                  || await userManager.FindByEmailAsync(email) is not null;
        if (exists)
            return Fail("That email address is already in use.");

        var isPrimary = !await context.UserEmails.AnyAsync(e => e.UserId == userId);

        var entry = new UserEmail
        {
            UserId     = userId,
            Email      = email,
            IsPrimary  = isPrimary,
            IsVerified = false,
            CreatedAt  = DateTime.UtcNow
        };
        await context.UserEmails.AddAsync(entry);
        await context.SaveChangesAsync();

        await otpService.IssueAsync(userId, displayName, email, OtpPurpose.EmailVerification);

        return Ok($"Email added. A verification code has been sent to {email}.");
    }

    public async Task<Result> VerifyEmailAsync(int userId, VerifyEmailDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var entry = await context.UserEmails
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Email == email);
        if (entry is null)
            return Fail("Email not found on this account.");
        if (entry.IsVerified)
            return Fail("Email is already verified.");

        var record = await otpService.VerifyAsync(userId, OtpPurpose.EmailVerification, dto.OtpCode);
        if (record is null)
            return Fail("Invalid or expired code.");

        entry.IsVerified = true;
        entry.VerifiedAt  = DateTime.UtcNow;

        // Sync primary email to AspNetUsers
        if (entry.IsPrimary)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is not null)
            {
                user.Email = email;
                await userManager.UpdateAsync(user);
            }
        }

        await context.SaveChangesAsync();
        return Ok("Email verified successfully.");
    }

    public async Task<Result> ResendEmailVerificationAsync(int userId, ResendEmailVerificationDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var entry = await context.UserEmails
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Email == email);
        if (entry is null) return Fail("Email not found on this account.");
        if (entry.IsVerified) return Fail("Email is already verified.");

        var user = await userManager.FindByIdAsync(userId.ToString());
        await otpService.IssueAsync(userId, user?.UserName ?? email, email, OtpPurpose.EmailVerification);
        return Ok("Verification code resent.");
    }

    public async Task<Result> SetPrimaryEmailAsync(int userId, SetPrimaryEmailDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var entry = await context.UserEmails
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Email == email);
        if (entry is null) return Fail("Email not found on this account.");
        if (!entry.IsVerified) return Fail("Please verify this email before setting it as primary.");
        if (entry.IsPrimary) return Fail("That email is already your primary email.");

        // Unset current primary
        var current = await context.UserEmails
            .Where(e => e.UserId == userId && e.IsPrimary)
            .ToListAsync();
        foreach (var e in current) e.IsPrimary = false;

        entry.IsPrimary = true;

        // Sync to AspNetUsers
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            user.Email = email;
            await userManager.UpdateAsync(user);
        }

        await context.SaveChangesAsync();
        return Ok($"{email} is now your primary email.");
    }

    public async Task<Result> RemoveEmailAsync(int userId, RemoveEmailDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var entry = await context.UserEmails
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Email == email);
        if (entry is null) return Fail("Email not found on this account.");
        if (entry.IsPrimary) return Fail("Cannot remove your primary email. Set another email as primary first.");

        entry.IsDeleted = true;
        await context.SaveChangesAsync();
        return Ok("Email removed.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PHONE
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<Result> AddPhoneAsync(int userId, string displayName, AddPhoneDto dto)
    {
        var phone = dto.PhoneNumber.Trim();
        var exists = await context.UserPhones.AnyAsync(p => p.PhoneNumber == phone);
        if (exists) return Fail("That phone number is already in use.");

        var isPrimary = !await context.UserPhones.AnyAsync(p => p.UserId == userId);

        var entry = new UserPhone
        {
            UserId      = userId,
            PhoneNumber = phone,
            IsPrimary   = isPrimary,
            IsVerified  = false,
            CreatedAt   = DateTime.UtcNow
        };
        await context.UserPhones.AddAsync(entry);
        await context.SaveChangesAsync();

        await otpService.IssueAsync(userId, displayName, phone, OtpPurpose.PhoneVerification);
        return Ok($"Phone added. A verification code has been sent to {MaskPhone(phone)}.");
    }

    public async Task<Result> VerifyPhoneAsync(int userId, VerifyPhoneDto dto)
    {
        var phone = dto.PhoneNumber.Trim();
        var entry = await context.UserPhones
            .FirstOrDefaultAsync(p => p.UserId == userId && p.PhoneNumber == phone);
        if (entry is null) return Fail("Phone not found on this account.");
        if (entry.IsVerified) return Fail("Phone is already verified.");

        var record = await otpService.VerifyAsync(userId, OtpPurpose.PhoneVerification, dto.OtpCode);
        if (record is null) return Fail("Invalid or expired code.");

        entry.IsVerified = true;
        entry.VerifiedAt  = DateTime.UtcNow;

        // Sync primary phone to AspNetUsers
        if (entry.IsPrimary)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is not null)
            {
                user.PhoneNumber = phone;
                user.PhoneNumberConfirmed = true;
                await userManager.UpdateAsync(user);
            }
        }

        await context.SaveChangesAsync();
        return Ok("Phone number verified successfully.");
    }

    public async Task<Result> ResendPhoneVerificationAsync(int userId, ResendPhoneVerificationDto dto)
    {
        var phone = dto.PhoneNumber.Trim();
        var entry = await context.UserPhones
            .FirstOrDefaultAsync(p => p.UserId == userId && p.PhoneNumber == phone);
        if (entry is null) return Fail("Phone not found on this account.");
        if (entry.IsVerified) return Fail("Phone is already verified.");

        var user = await userManager.FindByIdAsync(userId.ToString());
        await otpService.IssueAsync(userId, user?.UserName ?? phone, phone, OtpPurpose.PhoneVerification);
        return Ok("Verification code resent.");
    }

    public async Task<Result> SetPrimaryPhoneAsync(int userId, SetPrimaryPhoneDto dto)
    {
        var phone = dto.PhoneNumber.Trim();
        var entry = await context.UserPhones
            .FirstOrDefaultAsync(p => p.UserId == userId && p.PhoneNumber == phone);
        if (entry is null) return Fail("Phone not found on this account.");
        if (!entry.IsVerified) return Fail("Please verify this phone before setting it as primary.");
        if (entry.IsPrimary) return Fail("That phone is already your primary phone.");

        var current = await context.UserPhones
            .Where(p => p.UserId == userId && p.IsPrimary).ToListAsync();
        foreach (var p in current) p.IsPrimary = false;

        entry.IsPrimary = true;

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is not null)
        {
            user.PhoneNumber = phone;
            user.PhoneNumberConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        await context.SaveChangesAsync();
        return Ok($"{MaskPhone(phone)} is now your primary phone.");
    }

    public async Task<Result> RemovePhoneAsync(int userId, RemovePhoneDto dto)
    {
        var phone = dto.PhoneNumber.Trim();
        var entry = await context.UserPhones
            .FirstOrDefaultAsync(p => p.UserId == userId && p.PhoneNumber == phone);
        if (entry is null) return Fail("Phone not found on this account.");
        if (entry.IsPrimary) return Fail("Cannot remove your primary phone. Set another phone as primary first.");

        entry.IsDeleted = true;
        await context.SaveChangesAsync();
        return Ok("Phone number removed.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private static Result Ok(string message)   => new() { Success = true,  Message = message };
    private static Result Fail(string message) => new() { Success = false, Message = message };

    private static string MaskPhone(string phone) =>
        phone.Length <= 4 ? phone : phone[..3] + new string('*', phone.Length - 6) + phone[^3..];
}