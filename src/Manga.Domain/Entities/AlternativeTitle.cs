using Manga.Domain.Common;

namespace Manga.Domain.Entities;

/// <summary>
/// Alternative/localized title for a manga series (e.g. Japanese, Korean, romanized).
/// </summary>
public class AlternativeTitle : BaseEntity
{
    /// <summary>FK to the parent manga series.</summary>
    public Guid MangaSeriesId { get; set; }

    /// <summary>The alternative title text (max 300 chars).</summary>
    public string Title { get; set; } = string.Empty;

    // Navigation properties
    public MangaSeries MangaSeries { get; set; } = null!;
}
