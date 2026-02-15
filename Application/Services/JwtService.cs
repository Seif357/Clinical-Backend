using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
using Domain.Models.Auth;
using Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class JwtService(IConfiguration configuration, ILogger<JwtService> logger,IOptions<JwtSettings> jwtSettings) : IJwtService
{
    
    public string GenerateAccessToken(List<Claim> claims)
    {
        logger.LogDebug("Generating access token for user: {UserId}", claims.GetUserId());

        var section = configuration.GetSection("JwtSettings");
        var secretKey = section["SecretKey"] ??
                        throw new InvalidOperationException("JWT SecretKey is not configured");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
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

        var refreshTokenExpirationDays = GetRefreshTokenExpirationDays();

        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(randomNumber),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays)
        };

        logger.LogDebug("Refresh token generated for user: {UserId}, expires in {Days} days",
            userId, refreshTokenExpirationDays);

        return refreshToken;
    }

    public int GetRefreshTokenExpirationDays()
    {
        return int.Parse(configuration.GetSection("JwtSettings")["RefreshTokenExpirationInDays"] ?? "7");
    }

    public int GetTokenExpirationMinutes()
    {
        return int.Parse(configuration.GetSection("JwtSettings")["ExpirationInMinutes"] ?? "60");
    }
}