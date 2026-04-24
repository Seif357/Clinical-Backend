using Domain.Models.Auth;
using Infrastructure.DataAccess;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DeviceRepository(AppDbContext context) : IDeviceRepository
{
    public Task<Device?> GetByIdAsync(int deviceId)
        => context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);

    public async Task<IReadOnlyList<Device>> GetUserDevicesAsync(int userId)
        => await context.Devices
            .Where(d => d.UserId == userId && !d.IsDeleted)
            .OrderByDescending(d => d.LastSeenAt)
            .ToListAsync();

    /// <summary>
    /// Matches an existing device by (userId + userAgent + ip).
    /// Creates a new one if no match is found.
    /// Caller must call SaveChangesAsync.
    /// </summary>
    public async Task<Device> GetOrCreateAsync(int userId, string userAgent, string ipAddress)
    {
        var existing = await context.Devices.FirstOrDefaultAsync(d =>
            d.UserId == userId &&
            d.UserAgent == userAgent &&
            d.IpAddress == ipAddress &&
            !d.IsDeleted);

        if (existing is not null)
        {
            existing.LastSeenAt = DateTime.UtcNow;
            return existing;
        }

        var device = new Device
        {
            UserId    = userId,
            UserAgent = userAgent,
            IpAddress = ipAddress,
            LastSeenAt = DateTime.UtcNow,
            CreatedAt  = DateTime.UtcNow
        };
        await context.Devices.AddAsync(device);
        return device;
    }

    public async Task UpdateLastSeenAsync(int deviceId)
    {
        var device = await context.Devices.FindAsync(deviceId);
        if (device is null) return;
        device.LastSeenAt = DateTime.UtcNow;
    }

    public async Task<bool> DeleteAsync(int deviceId, int userId)
    {
        var device = await context.Devices
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.UserId == userId);
        if (device is null) return false;
        device.IsDeleted = true;
        return true;
    }
}