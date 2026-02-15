using Manga.Domain.Common;

namespace Manga.Domain.Entities;

/// <summary>
/// User comment with polymorphic targeting (manga, chapter, or page-level).
/// Supports self-referencing replies via ParentId.
/// </summary>
public class Comment : AuditableEntity
{
    /// <summary>FK to the comment author.</summary>
    public Guid UserId { get; set; }

    /// <summary>Comment text content.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Denormalized like count for fast display.</summary>
    public int Likes { get; set; }

    /// <summary>Denormalized dislike count for fast display.</summary>
    public int Dislikes { get; set; }

    // Polymorphic target — at least one should be set
    /// <summary>FK to manga series (nullable). Set when commenting on a manga.</summary>
    public Guid? MangaSeriesId { get; set; }

    /// <summary>FK to chapter (nullable). Set when commenting on a specific chapter.</summary>
    public Guid? ChapterId { get; set; }

    /// <summary>Page number (nullable). Set when commenting on a specific page within a chapter.</summary>
    public int? PageNumber { get; set; }

    // Self-referencing replies
    /// <summary>FK to parent comment (nullable). Null for top-level comments.</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Denormalized count of direct replies.</summary>
    public int ReplyCount { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public MangaSeries? MangaSeries { get; set; }
    public Chapter? Chapter { get; set; }
    public Comment? Parent { get; set; }
    public ICollection<Comment> Replies { get; set; } = [];
    public ICollection<CommentReaction> Reactions { get; set; } = [];
}
