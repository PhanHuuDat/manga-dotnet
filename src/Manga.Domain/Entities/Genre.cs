using Manga.Domain.Common;

namespace Manga.Domain.Entities;

/// <summary>
/// Manga genre/category tag (e.g. Action, Romance, Fantasy).
/// Seeded with 12 default genres matching the frontend.
/// </summary>
public class Genre : BaseEntity
{
    /// <summary>Display name of the genre (unique, max 50 chars).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-friendly slug (unique, max 50 chars).</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Optional short description of the genre.</summary>
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<MangaGenre> MangaGenres { get; set; } = [];
}
