namespace Manga.Application.Auth.DTOs;

/// <summary>
/// Internal result from login/refresh handlers. API layer reads RefreshToken for cookie setting.
/// </summary>
public record LoginTokenResult(
    AuthResponse Auth,
    string RawRefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
