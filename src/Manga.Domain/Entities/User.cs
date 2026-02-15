using Manga.Domain.Common;
using Manga.Domain.Enums;

namespace Manga.Domain.Entities;

/// <summary>
/// Registered platform user. Supports authentication, role-based access, and activity tracking.
/// </summary>
public class User : AuditableEntity
{
    /// <summary>Unique login name (max 50 chars).</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Unique email address used for authentication and notifications.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Bcrypt/Argon2 hashed password — never stored in plaintext.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Optional public display name shown in comments and profiles.</summary>
    public string? DisplayName { get; set; }

    /// <summary>FK to avatar image attachment.</summary>
    public Guid? AvatarId { get; set; }

    /// <summary>User level based on activity/engagement. Defaults to 1.</summary>
    public int Level { get; set; } = 1;

    /// <summary>Whether the user is currently active on the platform.</summary>
    public bool IsOnline { get; set; }

    // Navigation properties
    public Attachment? Avatar { get; set; }
    public ICollection<UserRoleMapping> UserRoles { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<CommentReaction> CommentReactions { get; set; } = [];
    public ICollection<Bookmark> Bookmarks { get; set; } = [];
    public ICollection<ReadingHistory> ReadingHistories { get; set; } = [];
}
