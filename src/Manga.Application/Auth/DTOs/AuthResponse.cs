namespace Manga.Application.Auth.DTOs;

public record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    string UserId,
    string Username,
    string? DisplayName);
