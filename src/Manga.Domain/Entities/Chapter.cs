using Manga.Domain.Common;

namespace Manga.Domain.Entities;

public class Chapter : AuditableEntity
{
    public Guid MangaSeriesId { get; set; }
    public decimal ChapterNumber { get; set; }
    public string? Title { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int Pages { get; set; }
    public long Views { get; set; }
    public DateTimeOffset PublishedAt { get; set; }

    // Navigation properties
    public MangaSeries MangaSeries { get; set; } = null!;
    public ICollection<ChapterPage> ChapterPages { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<ReadingHistory> ReadingHistories { get; set; } = [];
}
