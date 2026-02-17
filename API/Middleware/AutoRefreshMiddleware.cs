using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Middleware;

/// <summary>
/// Intercepts requests with expired access tokens, silently refreshes them,
/// and injects the new access token into the response header: X-New-Access-Token.
/// The frontend should check for this header on every response and save it if present.
/// </summary>
public class AutoRefreshMiddleware(RequestDelegate next, ILogger<AutoRefreshMiddleware> logger)
{
    // These paths should never trigger auto-refresh (they handle tokens themselves)
    private static readonly HashSet<string> _excludedPaths =
    [
        "/api/auth/login",
        "/api/auth/register",
        "/api/auth/refresh-token",
        "/api/auth/logout"
    ];

    public async Task InvokeAsync(HttpContext context, IAuthService authService, IJwtService jwtService)
    {
        // Skip excluded paths
        if (_excludedPaths.Contains(context.Request.Path.Value?.ToLower() ?? string.Empty))
        {
            await next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.ToString();

        // Only intercept Bearer token requests
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var accessToken = authHeader["Bearer ".Length..].Trim();

        // Only intercept if the token is actually expired (not missing/malformed)
        if (!IsTokenExpired(accessToken, jwtService))
        {
            await next(context);
            return;
        }

        // Try to get the refresh token from cookie
        var refreshToken = context.Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            // No refresh token available — let the request fail normally (401)
            await next(context);
            return;
        }

        logger.LogInformation("Access token expired for request {Path} — attempting silent refresh.", context.Request.Path);

        try
        {
            var refreshResult = await authService.RefreshTokenAsync(refreshToken);

            if (!refreshResult.Success)
            {
                logger.LogWarning("Silent token refresh failed: {Message}", refreshResult.Message);
                await next(context);
                return;
            }

            // ✅ Inject new tokens back
            // New refresh token → cookie (same as your /refresh-token endpoint does)
            context.Response.Cookies.Append("refreshToken", refreshResult.RefreshToken!, new CookieOptions
            {
                HttpOnly = true,
                Secure = !IsDevEnvironment(context),
                SameSite = SameSiteMode.Lax,
                Expires = refreshResult.RefreshTokenExpiration,
                Path = "/",
                IsEssential = true
            });

            // New access token → response header (frontend reads this and saves it)
            context.Response.Headers["X-New-Access-Token"] = refreshResult.AccessToken;
            context.Response.Headers["X-New-Access-Token-Expiration"] =
                refreshResult.AccessTokenExpiration.ToString(); // ISO 8601

            // Expose the header to browsers (required for CORS clients to read it)
            context.Response.Headers.Append("Access-Control-Expose-Headers", "X-New-Access-Token, X-New-Access-Token-Expiration");

            // Re-inject the new access token into the current request so the
            // rest of the pipeline authenticates successfully
            context.Request.Headers.Authorization = $"Bearer {refreshResult.AccessToken}";

            logger.LogInformation("Silent token refresh succeeded for request {Path}.", context.Request.Path);
        }
        catch (SecurityTokenException ex)
        {
            logger.LogWarning("Silent token refresh threw SecurityTokenException: {Message}", ex.Message);
            // Fall through — let the pipeline return 401 naturally
        }

        await next(context);
    }

    /// <summary>
    /// Returns true only if the token is structurally valid but past its expiry.
    /// Malformed tokens return false so the pipeline handles them normally.
    /// </summary>
    private static bool IsTokenExpired(string token, IJwtService jwtService)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token)) return false;

            var jwt = handler.ReadJwtToken(token);

            // exp claim is in Unix seconds
            return jwt.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsDevEnvironment(HttpContext context) =>
        context.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment();
}