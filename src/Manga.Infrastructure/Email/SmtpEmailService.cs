using Manga.Domain.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Manga.Infrastructure.Email;

/// <summary>
/// Production email service using MailKit SMTP.
/// </summary>
public class SmtpEmailService(
    IOptions<EmailSettings> settings,
    ILogger<SmtpEmailService> logger) : IEmailService
{
    private readonly EmailSettings _settings = settings.Value;

    public async Task SendEmailVerificationAsync(string email, string username, string token, Guid userId, CancellationToken ct = default)
    {
        var url = $"{_settings.FrontendBaseUrl}/verify-email?token={Uri.EscapeDataString(token)}&userId={userId}";
        var (subject, body) = EmailTemplates.GetVerificationEmail(username, url);
        await SendAsync(email, subject, body, ct);
    }

    public async Task SendPasswordResetAsync(string email, string username, string token, Guid userId, CancellationToken ct = default)
    {
        var url = $"{_settings.FrontendBaseUrl}/reset-password?token={Uri.EscapeDataString(token)}&userId={userId}";
        var (subject, body) = EmailTemplates.GetPasswordResetEmail(username, url);
        await SendAsync(email, subject, body, ct);
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            var socketOptions = _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, socketOptions, ct);

            if (!string.IsNullOrEmpty(_settings.SmtpUsername))
                await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword, ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        }
    }
}
