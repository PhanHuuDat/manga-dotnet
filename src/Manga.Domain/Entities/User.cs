using Manga.Domain.Common;
using Manga.Domain.Enums;

namespace Manga.Domain.Entities;

public class User : AuditableEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public int Level { get; set; } = 1;
    public bool IsOnline { get; set; }
    public UserRole Role { get; set; } = UserRole.Reader;

    // Navigation properties
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<CommentReaction> CommentReactions { get; set; } = [];
    public ICollection<Bookmark> Bookmarks { get; set; } = [];
    public ICollection<ReadingHistory> ReadingHistories { get; set; } = [];
}
