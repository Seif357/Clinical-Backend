using Application.Interfaces;
using Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

/// <summary>
/// Twilio-backed SMS sender.
/// Replace the HttpClient call with the Twilio SDK if you add the NuGet package.
/// </summary>
public class SmsService(
    IOptions<TwilioSettings> twilioOptions,
    ILogger<SmsService> logger) : ISmsService
{
    private readonly TwilioSettings _twilio = twilioOptions.Value;

    public async Task SendOtpAsync(string phoneNumber, string otpCode, int expiryMinutes)
    {
        // ── Twilio REST approach (no SDK required) ────────────────────────────
        var credentials = Convert.ToBase64String(
            System.Text.Encoding.ASCII.GetBytes($"{_twilio.AccountSid}:{_twilio.AuthToken}"));

        var body = $"Your Clinical code is {otpCode}. It expires in {expiryMinutes} min. Do not share it.";

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

        var payload = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["To"]   = phoneNumber,
            ["From"] = _twilio.FromNumber,
            ["Body"] = body
        });

        var response = await http.PostAsync(
            $"https://api.twilio.com/2010-04-01/Accounts/{_twilio.AccountSid}/Messages.json",
            payload);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("Twilio SMS failed for {Phone}: {Error}", phoneNumber, error);
            throw new InvalidOperationException("Failed to send SMS. Please try again.");
        }

        logger.LogInformation("OTP SMS sent to {Phone}", phoneNumber);
    }
}