namespace Manga.Domain.Entities;

/// <summary>
/// Many-to-many join table linking MangaSeries to Genre.
/// Uses composite PK (MangaSeriesId, GenreId) — no BaseEntity inheritance.
/// </summary>
public class MangaGenre
{
    /// <summary>FK to the manga series.</summary>
    public Guid MangaSeriesId { get; set; }

    /// <summary>FK to the genre.</summary>
    public Guid GenreId { get; set; }

    // Navigation properties
    public MangaSeries MangaSeries { get; set; } = null!;
    public Genre Genre { get; set; } = null!;
}
