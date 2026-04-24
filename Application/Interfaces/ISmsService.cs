namespace Application.Interfaces;

public interface ISmsService
{
    Task SendOtpAsync(string phoneNumber, string otpCode, int expiryMinutes);
}