using Domain.Models;
using Domain.Models.Auth;

namespace Infrastructure.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetUserTokenAsync(string token);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(int userId);
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task RevokeAllUserTokensAsync(int userId, string reason);
    Task RevokeAllDeviceTokensAsync(int userId, int deviceId, string reason);
    Task<bool> IsTokenValidAsync(string token);


}

