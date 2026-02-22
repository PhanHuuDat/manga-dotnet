namespace Manga.Application.Admin.Queries.ListUsers;

/// <summary>
/// User summary for the admin user management table.
/// </summary>
public record AdminUserDto(
    Guid Id,
    string Username,
    string Email,
    string? DisplayName,
    string? AvatarUrl,
    bool IsActive,
    bool EmailConfirmed,
    List<string> Roles,
    DateTimeOffset CreatedDate);
