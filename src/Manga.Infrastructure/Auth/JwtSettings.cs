namespace Manga.Infrastructure.Auth;

/// <summary>
/// JWT configuration bound from appsettings "Jwt" section.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>HMAC-SHA256 signing key — minimum 32 characters (256-bit).</summary>
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
