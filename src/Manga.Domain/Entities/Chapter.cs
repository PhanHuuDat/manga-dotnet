using Manga.Domain.Common;

namespace Manga.Domain.Entities;

/// <summary>
/// A single chapter within a manga series, containing ordered pages.
/// </summary>
public class Chapter : AuditableEntity
{
    /// <summary>FK to the parent manga series.</summary>
    public Guid MangaSeriesId { get; set; }

    /// <summary>Chapter number. Uses decimal(6,1) to support .5 chapters (e.g. 10.5).</summary>
    public decimal ChapterNumber { get; set; }

    /// <summary>Optional chapter title (e.g. "The Final Battle").</summary>
    public string? Title { get; set; }

    /// <summary>URL-friendly slug for this chapter.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Denormalized page count for quick display.</summary>
    public int Pages { get; set; }

    /// <summary>Cumulative view count for this chapter.</summary>
    public long Views { get; set; }

    /// <summary>When this chapter was published/released.</summary>
    public DateTimeOffset PublishedAt { get; set; }

    // Navigation properties
    public MangaSeries MangaSeries { get; set; } = null!;
    public ICollection<ChapterPage> ChapterPages { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<ReadingHistory> ReadingHistories { get; set; } = [];
}
