namespace Domain.Constants;

public class RevokeConstants
{
    public static class Messages
    {
        public const string OldTokenUsage = "Reuse of a revoked refresh token";
        public const string RefreshTokenReplaced = "Refresh token replaced by a new one";
        public const string UserDeleted = "the user of this Refresh token was deleted";
    }
}