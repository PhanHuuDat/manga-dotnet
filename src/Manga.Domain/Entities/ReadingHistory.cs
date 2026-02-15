using Manga.Domain.Common;

namespace Manga.Domain.Entities;

public class ReadingHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MangaSeriesId { get; set; }
    public Guid ChapterId { get; set; }
    public int LastPageNumber { get; set; } = 1;
    public DateTimeOffset LastReadAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public MangaSeries MangaSeries { get; set; } = null!;
    public Chapter Chapter { get; set; } = null!;
}
