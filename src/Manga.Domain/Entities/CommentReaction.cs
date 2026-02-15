using Manga.Domain.Common;
using Manga.Domain.Enums;

namespace Manga.Domain.Entities;

/// <summary>
/// User reaction (like/dislike) on a comment. One reaction per user per comment (unique constraint).
/// </summary>
public class CommentReaction : BaseEntity
{
    /// <summary>FK to the user who reacted.</summary>
    public Guid UserId { get; set; }

    /// <summary>FK to the target comment.</summary>
    public Guid CommentId { get; set; }

    /// <summary>Reaction type: Like or Dislike.</summary>
    public ReactionType ReactionType { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Comment Comment { get; set; } = null!;
}
