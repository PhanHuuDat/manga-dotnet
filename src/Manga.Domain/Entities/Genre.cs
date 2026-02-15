using Manga.Domain.Common;

namespace Manga.Domain.Entities;

public class Genre : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<MangaGenre> MangaGenres { get; set; } = [];
}
