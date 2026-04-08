namespace Domain.Constants;

public static class AuthConstants
{
    public static class Messages
    {
        public const string InvalidCredentials = "Invalid Credentials!";
        public const string UserNotFound = "User not found";
        public const string EmailAlreadyExists = "Email already exists!";
        public const string UsernameAlreadyExists = "Username already exists!";
        public const string UserRegisteredSuccessfully = "Successfully registered new user!";
        public const string LoginSuccessful = "Login successful";
        public const string TokenRefreshedSuccessfully = "Successfully Refreshed Token";
        public const string EmailAlreadyVerified = "Email already verified";
        public const string EmailVerificationSent = "Email verification link sent to your email";
        public const string PhoneAlreadyExists = "Phone number already exists!";
        public const string GoogleAuthFailed = "Google authentication failed. Please try again.";
        public const string LoginFailed = "Login failed";
    }
}