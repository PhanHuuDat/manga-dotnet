using Manga.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Manga.Infrastructure.Email;

/// <summary>
/// Development email service — logs email content to console instead of sending.
/// </summary>
public class DevEmailService(
    IOptions<EmailSettings> settings,
    ILogger<DevEmailService> logger) : IEmailService
{
    private readonly EmailSettings _settings = settings.Value;

    public Task SendEmailVerificationAsync(string email, string username, string token, Guid userId, CancellationToken ct = default)
    {
        var url = $"{_settings.FrontendBaseUrl}/verify-email?token={Uri.EscapeDataString(token)}&userId={userId}";
        logger.LogInformation("[DEV EMAIL] To: {Email} | Subject: Verify your LuvManga email", email);
        logger.LogInformation("[DEV EMAIL] Verification URL: {Url}", url);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string username, string token, Guid userId, CancellationToken ct = default)
    {
        var url = $"{_settings.FrontendBaseUrl}/reset-password?token={Uri.EscapeDataString(token)}&userId={userId}";
        logger.LogInformation("[DEV EMAIL] To: {Email} | Subject: Reset your LuvManga password", email);
        logger.LogInformation("[DEV EMAIL] Reset URL: {Url}", url);
        return Task.CompletedTask;
    }
}
