using Manga.Domain.Common;

namespace Manga.Domain.Entities;

public class Bookmark : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid MangaSeriesId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public MangaSeries MangaSeries { get; set; } = null!;
}
