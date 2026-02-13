using Domain.Models;
using Domain.Models.Auth;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetUserTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(int userId)
    {
        var now = DateTime.UtcNow;
        return await _context.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.UserId == userId
            && rt.RevokedAt == null 
            && rt.ExpiresAt > now)
            .ToListAsync();
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        if (refreshToken == null)
            throw new ArgumentNullException(nameof(refreshToken));

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        if (refreshToken == null)
            throw new ArgumentNullException(nameof(refreshToken));

        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllUserTokensAsync(int userId, string reason)
    {
        var now = DateTime.UtcNow;
        await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(rt => rt.RevokedAt, now)
                .SetProperty(rt => rt.ReasonRevoked, reason));

        await _context.SaveChangesAsync();
    }
    public async Task RevokeAllDeviceTokensAsync(int userId, int deviceId, string reason)
    {
        var now = DateTime.UtcNow;
        await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.DeviceId == deviceId && rt.RevokedAt == null && rt.ExpiresAt > now)
                .ExecuteUpdateAsync(s => s
                .SetProperty(rt => rt.RevokedAt, now)
                .SetProperty(rt => rt.ReasonRevoked, reason));

        await _context.SaveChangesAsync();
    }
    public async Task<bool> IsTokenValidAsync(string token)
    {
        var now = DateTime.UtcNow;

        return await _context.RefreshTokens
            .AsNoTracking()
            .Include(rt => rt.User)
            .AnyAsync(rt => rt.Token == token
                && rt.RevokedAt == null
                && rt.ExpiresAt > now 
                && rt.User != null 
                && rt.IsActive == true);
    }
}

