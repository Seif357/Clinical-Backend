using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
using Domain.Models.Auth;
using Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class JwtService(ILogger<JwtService> logger,IOptions<JwtSettings> jwtSettings,TokenValidationParameters tokenValidationParameters) : IJwtService
{
    
    public string GenerateAccessToken(List<Claim> claims)
    {
        logger.LogDebug("Generating access token for user: {UserId}", claims.GetUserId());
        

        var credentials = new SigningCredentials(tokenValidationParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            jwtSettings.Value.Issuer,
            jwtSettings.Value.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.Value.ExpirationInMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        logger.LogDebug("Access token generated successfully for user: {UserId}, expires in {Minutes} minutes",
            claims.GetUserId(), jwtSettings.Value.ExpirationInMinutes);

        return tokenString;
    }

    public RefreshToken GenerateRefreshToken(int userId)
    {
        logger.LogDebug("Generating refresh token for user: {UserId}", userId);

        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);


        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpirationInDays)
        };

        logger.LogDebug("Refresh token generated for user: {UserId}, expires in {Days} days",
            userId, jwtSettings.Value.RefreshTokenExpirationInDays);

        return refreshToken;
    }
    /// <summary>
    /// Reuses the singleton TokenValidationParameters registered in DI,
    /// with ValidateLifetime overridden to false — so only genuinely expired
    /// but otherwise valid tokens return true, making them candidates for silent refresh.
    /// Tampered or malformed tokens return false and fall through to the normal 401 pipeline.
    /// </summary>
    public async Task<TokenValidationStatus> IsTokenExpiredAsync(string token)
    {
            var handler = new JwtSecurityTokenHandler();
            var result = await handler.ValidateTokenAsync(token, tokenValidationParameters);

            if (result.IsValid)
                return TokenValidationStatus.Valid;

            return result.Exception is SecurityTokenExpiredException
                ? TokenValidationStatus.Expired
                : TokenValidationStatus.Invalid;
    }
    public int GetRefreshTokenExpirationDays()
    {
        return jwtSettings.Value.RefreshTokenExpirationInDays;
    }

    public int GetTokenExpirationMinutes()
    {
        return jwtSettings.Value.ExpirationInMinutes;
    }
}