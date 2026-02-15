using Manga.Domain.Common;

namespace Manga.Domain.Entities;

/// <summary>
/// A single page/image within a chapter, ordered by page number.
/// </summary>
public class ChapterPage : BaseEntity
{
    /// <summary>FK to the parent chapter.</summary>
    public Guid ChapterId { get; set; }

    /// <summary>Sequential page number within the chapter (1-based). Unique per chapter.</summary>
    public int PageNumber { get; set; }

    /// <summary>FK to the page image attachment.</summary>
    public Guid ImageId { get; set; }

    // Navigation properties
    public Chapter Chapter { get; set; } = null!;
    public Attachment Image { get; set; } = null!;
}
