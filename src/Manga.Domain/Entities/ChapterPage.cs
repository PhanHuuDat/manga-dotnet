using Manga.Domain.Common;

namespace Manga.Domain.Entities;

public class ChapterPage : BaseEntity
{
    public Guid ChapterId { get; set; }
    public int PageNumber { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    // Navigation properties
    public Chapter Chapter { get; set; } = null!;
}
