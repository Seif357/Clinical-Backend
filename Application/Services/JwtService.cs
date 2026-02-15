using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Interfaces;
using Domain.Models.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    public string GenerateAccessToken(List<Claim> claims)
    {
        _logger.LogDebug("Generating access token for user: {UserId}", claims.GetUserId());

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ??
                        throw new InvalidOperationException("JWT SecretKey is not configured");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationInMinutes"] ?? "60");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        _logger.LogDebug("Access token generated successfully for user: {UserId}, expires in {Minutes} minutes",
            claims.GetUserId(), expirationMinutes);

        return tokenString;
    }

    public RefreshToken GenerateRefreshToken(int userId)
    {
        _logger.LogDebug("Generating refresh token for user: {UserId}", userId);

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

        _logger.LogDebug("Refresh token generated for user: {UserId}, expires in {Days} days",
            userId, refreshTokenExpirationDays);

        return refreshToken;
    }

    public int GetRefreshTokenExpirationDays()
    {
        return int.Parse(_configuration.GetSection("JwtSettings")["RefreshTokenExpirationInDays"] ?? "7");
    }

    public int GetTokenExpirationMinutes()
    {
        return int.Parse(_configuration.GetSection("JwtSettings")["ExpirationInMinutes"] ?? "60");
    }
}