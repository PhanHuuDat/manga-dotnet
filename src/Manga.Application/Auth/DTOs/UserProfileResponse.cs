namespace Manga.Application.Auth.DTOs;

public record UserProfileResponse(
    Guid Id,
    string Username,
    string Email,
    string? DisplayName,
    string? AvatarUrl,
    int Level,
    bool EmailConfirmed,
    IReadOnlyList<string> Roles);
