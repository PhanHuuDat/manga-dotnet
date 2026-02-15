using Manga.Domain.Common;
using Manga.Domain.Enums;

namespace Manga.Domain.Entities;

/// <summary>
/// Core aggregate representing a manga title with metadata, stats, and relationships.
/// Named MangaSeries to avoid C# namespace collision with the Manga root namespace.
/// </summary>
public class MangaSeries : AuditableEntity
{
    /// <summary>Primary title of the manga series.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Auto-incremented identifier for human-friendly URLs (e.g. /manga/123).</summary>
    public int SeriesNumber { get; set; }

    /// <summary>Long-form description/summary of the manga plot.</summary>
    public string? Synopsis { get; set; }

    /// <summary>FK to cover image attachment.</summary>
    public Guid? CoverId { get; set; }

    /// <summary>FK to banner image attachment.</summary>
    public Guid? BannerId { get; set; }

    /// <summary>FK to the original author/writer.</summary>
    public Guid AuthorId { get; set; }

    /// <summary>FK to the illustrator/artist, if different from the author.</summary>
    public Guid? ArtistId { get; set; }

    /// <summary>Publication status: Ongoing, Completed, or Hiatus.</summary>
    public SeriesStatus Status { get; set; } = SeriesStatus.Ongoing;

    /// <summary>Optional promotional badge: Hot, Top, or New.</summary>
    public MangaBadge? Badge { get; set; }

    /// <summary>Year the manga was first published.</summary>
    public int? PublishedYear { get; set; }

    /// <summary>Average user rating (0.00–5.00). Precision: decimal(3,2).</summary>
    public decimal Rating { get; set; }

    /// <summary>Total number of user ratings submitted.</summary>
    public int RatingCount { get; set; }

    /// <summary>Cumulative page/chapter view count across all chapters.</summary>
    public long Views { get; set; }

    /// <summary>Denormalized count of published chapters for fast display.</summary>
    public int TotalChapters { get; set; }

    /// <summary>Denormalized latest chapter number for quick reference.</summary>
    public int LatestChapterNumber { get; set; }

    // Navigation properties
    public Attachment? Cover { get; set; }
    public Attachment? Banner { get; set; }
    public Person Author { get; set; } = null!;
    public Person? Artist { get; set; }
    public ICollection<AlternativeTitle> AlternativeTitles { get; set; } = [];
    public ICollection<MangaGenre> MangaGenres { get; set; } = [];
    public ICollection<Chapter> Chapters { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Bookmark> Bookmarks { get; set; } = [];
    public ICollection<ReadingHistory> ReadingHistories { get; set; } = [];
}
