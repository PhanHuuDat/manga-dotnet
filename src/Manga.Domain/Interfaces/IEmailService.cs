namespace Manga.Domain.Interfaces;

/// <summary>
/// Email sending contract. Implementations: SmtpEmailService (prod), DevEmailService (dev).
/// </summary>
public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string username, string token, Guid userId, CancellationToken ct = default);
    Task SendPasswordResetAsync(string email, string username, string token, Guid userId, CancellationToken ct = default);
}
