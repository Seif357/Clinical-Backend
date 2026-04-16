namespace API.Helpers;

public static class AuthHelper
{
    public static void SetRefreshTokenCookie(HttpResponse response, string refreshToken,
        DateTime? expiration, IWebHostEnvironment environment, int refreshTokenExpirationDays)
    {
        response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = expiration ?? DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
            Path = "/",
            IsEssential = true
        });
    }

    public static void DeleteRefreshTokenCookie(HttpResponse response, IWebHostEnvironment environment)
    {
        response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(-1),
            Path = "/"
        });
    }
}