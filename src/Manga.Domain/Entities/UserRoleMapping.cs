using Manga.Domain.Enums;

namespace Manga.Domain.Entities;

/// <summary>
/// Join table mapping users to their roles. A user can have multiple roles.
/// No BaseEntity inheritance — composite PK (UserId, Role), no GUID needed.
/// </summary>
public class UserRoleMapping
{
    public Guid UserId { get; set; }
    public UserRole Role { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}
