using Manga.Domain.Common;

namespace Manga.Domain.Entities;

public class AlternativeTitle : BaseEntity
{
    public Guid MangaSeriesId { get; set; }
    public string Title { get; set; } = string.Empty;

    // Navigation properties
    public MangaSeries MangaSeries { get; set; } = null!;
}
