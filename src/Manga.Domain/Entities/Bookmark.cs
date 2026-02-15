using Manga.Domain.Common;

namespace Manga.Domain.Entities;

/// <summary>
/// User's bookmarked/saved manga series for their personal library.
/// One bookmark per user per manga (unique constraint).
/// </summary>
public class Bookmark : AuditableEntity
{
    /// <summary>FK to the user who bookmarked.</summary>
    public Guid UserId { get; set; }

    /// <summary>FK to the bookmarked manga series.</summary>
    public Guid MangaSeriesId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public MangaSeries MangaSeries { get; set; } = null!;
}
