using Manga.Domain.Common;
using Manga.Domain.Enums;

namespace Manga.Domain.Entities;

public class CommentReaction : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CommentId { get; set; }
    public ReactionType ReactionType { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Comment Comment { get; set; } = null!;
}
