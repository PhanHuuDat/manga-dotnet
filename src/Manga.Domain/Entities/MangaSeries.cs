using Manga.Domain.Common;
using Manga.Domain.Enums;

namespace Manga.Domain.Entities;

public class MangaSeries : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Synopsis { get; set; }
    public string? CoverUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string Author { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public SeriesStatus Status { get; set; } = SeriesStatus.Ongoing;
    public MangaBadge? Badge { get; set; }
    public int? PublishedYear { get; set; }
    public decimal Rating { get; set; }
    public int RatingCount { get; set; }
    public long Views { get; set; }
    public int TotalChapters { get; set; }
    public int LatestChapterNumber { get; set; }

    // Navigation properties
    public ICollection<AlternativeTitle> AlternativeTitles { get; set; } = [];
    public ICollection<MangaGenre> MangaGenres { get; set; } = [];
    public ICollection<Chapter> Chapters { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Bookmark> Bookmarks { get; set; } = [];
    public ICollection<ReadingHistory> ReadingHistories { get; set; } = [];
}
