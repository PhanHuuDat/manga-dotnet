namespace Manga.Infrastructure.Email;

/// <summary>
/// Email configuration bound from appsettings "Email" section.
/// </summary>
public class EmailSettings
{
    public const string SectionName = "Email";

    public bool UseDev { get; set; } = true;
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@luvmanga.com";
    public string FromName { get; set; } = "LuvManga";
    public bool UseSsl { get; set; } = true;
    public string FrontendBaseUrl { get; set; } = "http://localhost:5173";
}
