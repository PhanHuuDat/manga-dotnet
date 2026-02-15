using Manga.Domain.Common;

namespace Manga.Domain.Entities;

/// <summary>
/// Tracks a user's reading progress for a manga series.
/// One record per user per manga (unique constraint) — updated as the user reads.
/// </summary>
public class ReadingHistory : BaseEntity
{
    /// <summary>FK to the reader.</summary>
    public Guid UserId { get; set; }

    /// <summary>FK to the manga series being read.</summary>
    public Guid MangaSeriesId { get; set; }

    /// <summary>FK to the last chapter the user was reading.</summary>
    public Guid ChapterId { get; set; }

    /// <summary>Last page number viewed in the chapter. Defaults to 1.</summary>
    public int LastPageNumber { get; set; } = 1;

    /// <summary>Timestamp of the user's most recent reading activity.</summary>
    public DateTimeOffset LastReadAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public MangaSeries MangaSeries { get; set; } = null!;
    public Chapter Chapter { get; set; } = null!;
}
