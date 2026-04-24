namespace Application.Interfaces;

public interface IEmailService
{
    Task SendOtpAsync(string toEmail, string displayName, string otpCode, string purpose, int expiryMinutes);
    Task SendAsync(string toEmail, string subject, string htmlBody);
}