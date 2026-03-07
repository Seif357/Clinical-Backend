using API.Helpers;
using Application.Interfaces;
using Domain.Models.Auth;

namespace API.Middleware;

/// <summary>
/// Intercepts requests with expired access tokens, silently refreshes them,
/// and injects the new access token into the response header: X-New-Access-Token.
/// The frontend should check for this header on every response and save it if present.
/// </summary>
public class AutoRefreshMiddleware(RequestDelegate next, ILogger<AutoRefreshMiddleware> logger)
{
    private static readonly HashSet<string> ExcludedPaths =
    [
        "/api/auth/login",
        "/api/auth/register",
        "/api/auth/refresh-token",
        "/api/auth/logout"
    ];

    public async Task InvokeAsync(HttpContext context, IAuthService authService, IJwtService jwtService,
        IWebHostEnvironment environment)
    {
        if (ExcludedPaths.Contains(context.Request.Path.Value?.ToLower() ?? string.Empty))
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

        var validationStatus = await jwtService.IsTokenExpiredAsync(accessToken);
        if (validationStatus != TokenValidationStatus.Expired)
        {
            await next(context);
            return;
        }

        var refreshToken = context.Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            await next(context);
            return;
        }

        logger.LogInformation("Access token expired for request {Path} — attempting silent refresh.",
            context.Request.Path);

        var refreshResult = await authService.RefreshTokenAsync(refreshToken);

        if (!refreshResult.Success)
        {
            logger.LogInformation("Silent refresh failed for {Path}: {Message}",
                context.Request.Path, refreshResult.Message);
            await next(context);
            return;
        }

        AuthHelper.SetRefreshTokenCookie(
            context.Response,
            refreshResult.RefreshToken,
            refreshResult.RefreshTokenExpiration,
            environment,
            jwtService.GetRefreshTokenExpirationDays());

        context.Response.Headers["X-New-Access-Token"] = refreshResult.AccessToken;
        context.Response.Headers["X-New-Access-Token-Expiration"] =
            refreshResult.AccessTokenExpiration?.ToString("O"); // ISO 8601

        context.Response.Headers.Append("Access-Control-Expose-Headers",
            "X-New-Access-Token, X-New-Access-Token-Expiration");

        context.Request.Headers.Authorization = $"Bearer {refreshResult.AccessToken}";
        logger.LogInformation("Silent token refresh succeeded for request {Path}.", context.Request.Path);

        await next(context);
    }
}