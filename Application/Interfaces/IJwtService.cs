using System.Security.Claims;
using Domain.Models.Auth;

namespace Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(List<Claim> claims);
    RefreshToken GenerateRefreshToken(int userId);
    int GetRefreshTokenExpirationDays();
    int GetTokenExpirationMinutes();
}