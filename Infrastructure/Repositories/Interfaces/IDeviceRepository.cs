using Domain.Models.Auth;

namespace Infrastructure.Repositories.Interfaces;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(int deviceId);
    Task<IReadOnlyList<Device>> GetUserDevicesAsync(int userId);
    Task<Device> GetOrCreateAsync(int userId, string userAgent, string ipAddress);
    Task UpdateLastSeenAsync(int deviceId);
    Task<bool> DeleteAsync(int deviceId, int userId);
}