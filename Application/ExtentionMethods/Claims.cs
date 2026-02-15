using System.Security.Claims;

public static class Claims
{
    public static string GetUserId(this IEnumerable<Claim> claims)
    {
        return claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value 
               ?? throw new InvalidOperationException("User ID claim not found");
    }
}