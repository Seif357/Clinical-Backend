using Domain.Models.Auth;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OtpRepository(AppDbContext context) : IOtpRepository
{
    public async Task AddAsync(OtpRecord record)
        => await context.OtpRecords.AddAsync(record);

    /// <summary>Marks all previous unused, unexpired OTPs for this user+purpose as used.</summary>
    public async Task InvalidatePreviousAsync(int userId, OtpPurpose purpose)
    {
        var previous = await context.OtpRecords
            .Where(o => o.UserId == userId && o.Purpose == purpose && !o.IsUsed)
            .ToListAsync();
        foreach (var o in previous) o.IsUsed = true;
    }

    public Task<OtpRecord?> GetActiveAsync(int userId, OtpPurpose purpose)
        => context.OtpRecords
            .Where(o => o.UserId == userId &&
                        o.Purpose == purpose &&
                        !o.IsUsed &&
                        o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

    public Task MarkUsedAsync(OtpRecord record)
    {
        record.IsUsed = true;
        return Task.CompletedTask;
    }
}