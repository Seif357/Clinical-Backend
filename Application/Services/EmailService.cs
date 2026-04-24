using Application.Interfaces;
using Infrastructure.Configurations;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services;

public class EmailService(
    IOptions<SmtpSettings> smtpOptions,
    ILogger<EmailService> logger) : IEmailService
{
    private readonly SmtpSettings _smtp = smtpOptions.Value;

    public Task SendOtpAsync(string toEmail, string displayName, string otpCode, string purpose, int expiryMinutes)
    {
        var subject = purpose switch
        {
            "ForgotPassword"             => "Reset your Clinical password",
            "PasswordChangeConfirmation" => "Confirm your Clinical password change",
            "EmailVerification"          => "Verify your Clinical email address",
            _                            => "Your Clinical verification code"
        };

        var html = $"""
            <div style="font-family:sans-serif;max-width:480px;margin:auto">
              <h2 style="color:#6366f1">Clinical</h2>
              <p>Hi {displayName},</p>
              <p>Your verification code is:</p>
              <div style="font-size:36px;font-weight:bold;letter-spacing:8px;margin:24px 0;color:#111">{otpCode}</div>
              <p>It expires in <strong>{expiryMinutes} minutes</strong>.</p>
              <p style="color:#888;font-size:12px">If you didn't request this, you can safely ignore this email.</p>
            </div>
            """;

        return SendAsync(toEmail, subject, html);
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtp.SenderName, _smtp.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtp.Username, _smtp.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            logger.LogInformation("Email sent to {Email} | Subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }
}