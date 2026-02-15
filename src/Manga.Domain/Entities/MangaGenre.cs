namespace Manga.Domain.Entities;

public class MangaGenre
{
    public Guid MangaSeriesId { get; set; }
    public Guid GenreId { get; set; }

    // Navigation properties
    public MangaSeries MangaSeries { get; set; } = null!;
    public Genre Genre { get; set; } = null!;
}
