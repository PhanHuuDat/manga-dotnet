using Manga.Domain.Common;

namespace Manga.Domain.Entities;

public class Comment : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Likes { get; set; }
    public int Dislikes { get; set; }

    // Polymorphic target
    public Guid? MangaSeriesId { get; set; }
    public Guid? ChapterId { get; set; }
    public int? PageNumber { get; set; }

    // Self-referencing replies
    public Guid? ParentId { get; set; }
    public int ReplyCount { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public MangaSeries? MangaSeries { get; set; }
    public Chapter? Chapter { get; set; }
    public Comment? Parent { get; set; }
    public ICollection<Comment> Replies { get; set; } = [];
    public ICollection<CommentReaction> Reactions { get; set; } = [];
}
