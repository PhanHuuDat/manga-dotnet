namespace Manga.Infrastructure.Email;

/// <summary>
/// Static HTML email templates for verification and password reset.
/// </summary>
public static class EmailTemplates
{
    public static (string Subject, string HtmlBody) GetVerificationEmail(string username, string url)
    {
        const string subject = "Verify your LuvManga email";
        var body = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#1a1a2e;color:#e0e0e0;padding:32px;border-radius:8px;">
                <h1 style="color:#e040fb;">LuvManga</h1>
                <p>Hi <strong>{username}</strong>,</p>
                <p>Thanks for registering! Please verify your email address by clicking the button below.</p>
                <div style="text-align:center;margin:24px 0;">
                    <a href="{url}" style="background:#e040fb;color:#fff;padding:12px 32px;text-decoration:none;border-radius:6px;font-weight:bold;">Verify Email</a>
                </div>
                <p style="font-size:12px;color:#888;">This link expires in 24 hours. If you didn't create this account, you can ignore this email.</p>
            </div>
            """;
        return (subject, body);
    }

    public static (string Subject, string HtmlBody) GetPasswordResetEmail(string username, string url)
    {
        const string subject = "Reset your LuvManga password";
        var body = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;background:#1a1a2e;color:#e0e0e0;padding:32px;border-radius:8px;">
                <h1 style="color:#e040fb;">LuvManga</h1>
                <p>Hi <strong>{username}</strong>,</p>
                <p>We received a request to reset your password. Click the button below to set a new one.</p>
                <div style="text-align:center;margin:24px 0;">
                    <a href="{url}" style="background:#e040fb;color:#fff;padding:12px 32px;text-decoration:none;border-radius:6px;font-weight:bold;">Reset Password</a>
                </div>
                <p style="font-size:12px;color:#888;">This link expires in 3 hours. If you didn't request this, you can safely ignore this email.</p>
            </div>
            """;
        return (subject, body);
    }
}
