using System.Security.Claims;
using Domain.Models.Auth;

namespace Application.Common.Interfaces;

public interface IJwtTokenService
{
    Task<(string token, DateTime expirationDate)> GenerateJwtTokenAsync(IEnumerable<Claim> claims);
    Task<RefreshToken> GenerateRefreshTokenAsync(int userId);
}
