using Application.Common.Interfaces;
using Domain.Models;
using Domain.Models.Auth;
using Infrastructure;
using Infrastructure.Configuration;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Application.Services;

public class JwtTokenService(
    IOptions<JwtSettings> jwtSettings,
    TokenValidationParameters tokenValidationParameters,
    AppDbContext context
   ) : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly AppDbContext _context= context;

    public async Task<(string token, DateTime expirationDate)> GenerateJwtTokenAsync(IEnumerable<Claim> claims)
    {
        try
        {
            var secretKey = _jwtSettings.SecretKey ?? throw new ApplicationException("JWT Secret Key is not configured");
            var issuer = _jwtSettings.ValidIssuer ?? throw new ApplicationException("JWT Issuer is not configured");
            var audience = _jwtSettings.ValidAudience ?? throw new ApplicationException("JWT Audience is not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var claimsList = claims.ToList();
            var roleClaim = claimsList.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
            var expiryMinutes = _jwtSettings.AdminExpiryMinutes > 0
                ? _jwtSettings.AdminExpiryMinutes
                : throw new ApplicationException("JWT Admin expiry minutes must be greater than 0");
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
            var fullClaims = AddStandardClaims(claimsList);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Subject = new ClaimsIdentity(fullClaims),
                NotBefore = DateTime.UtcNow
            };
            var tokenHandler = new JsonWebTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            if (!(await tokenHandler.ValidateTokenAsync(token, tokenValidationParameters)).IsValid)
            {
                throw new SecurityTokenValidationException("Generated jwt Failed Validation!");
            }

            return (token, expiresAt);
        }
        catch (Exception e)
        {
            Console.WriteLine("--------------------------------------------------------------------------------------------------------------------------"+e);
            throw;
        }
    }
    private IEnumerable<Claim> AddStandardClaims(IEnumerable<Claim> claims)
    {
        var enhancedClaims = claims.ToList();

        if (enhancedClaims.All(c => c.Type != JwtRegisteredClaimNames.Jti))
        {
            enhancedClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        }

        if (enhancedClaims.All(c => c.Type != JwtRegisteredClaimNames.Iat))
        {
            enhancedClaims.Add(new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()));
        }

        return enhancedClaims;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(int userId)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(_jwtSettings.RefreshTokenExpiryHours)
        };
        // ToDo: Removing Old Tokens
        // *note*: Consider implementing a scheduled cleanUp Service 

        await _context.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

}