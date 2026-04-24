using Application.Interfaces;
using Domain.Models.Auth;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class OtpService(
    IOtpRepository otpRepository,
    IEmailService emailService,
    ISmsService smsService,
    IConfiguration configuration,
    AppDbContext context) : IOtpService
{
    private static readonly Random _rng = new();
    private int ExpiryMinutes => configuration.GetValue<int>("Otp:ExpiryMinutes", 10);

    public async Task<string> IssueAsync(
        int userId,
        string displayName,
        string target,
        OtpPurpose purpose,
        string? pendingPasswordHash = null)
    {
        // Invalidate any previous pending OTPs for this purpose
        await otpRepository.InvalidatePreviousAsync(userId, purpose);

        // Generate 6-digit code
        var plainCode = _rng.Next(100_000, 999_999).ToString();
        var hash = BCrypt.Net.BCrypt.HashPassword(plainCode);

        var record = new OtpRecord
        {
            UserId              = userId,
            Purpose             = purpose,
            Target              = target,
            CodeHash            = hash,
            ExpiresAt           = DateTime.UtcNow.AddMinutes(ExpiryMinutes),
            IsUsed              = false,
            PendingPasswordHash = pendingPasswordHash,
            CreatedAt           = DateTime.UtcNow
        };

        await otpRepository.AddAsync(record);
        await context.SaveChangesAsync();

        // Dispatch
        bool isPhone = target.StartsWith('+') || target.All(c => char.IsDigit(c) || c == '+' || c == '-');
        if (isPhone)
            await smsService.SendOtpAsync(target, plainCode, ExpiryMinutes);
        else
            await emailService.SendOtpAsync(target, displayName, plainCode, purpose.ToString(), ExpiryMinutes);

        return plainCode; // only returned for tests; caller should not expose it
    }

    public async Task<OtpRecord?> VerifyAsync(int userId, OtpPurpose purpose, string plainCode)
    {
        var record = await otpRepository.GetActiveAsync(userId, purpose);
        if (record is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(plainCode, record.CodeHash)) return null;

        await otpRepository.MarkUsedAsync(record);
        await context.SaveChangesAsync();
        return record;
    }
}