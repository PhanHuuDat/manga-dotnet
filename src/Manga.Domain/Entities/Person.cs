using Manga.Domain.Common;

namespace Manga.Domain.Entities;

/// <summary>
/// Represents an author or artist associated with manga series.
/// </summary>
public class Person : AuditableEntity
{
    /// <summary>Full name of the person.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Auto-incremented identifier for human-friendly URLs (e.g. /author/42).</summary>
    public int PersonNumber { get; set; }

    /// <summary>Optional biographical text.</summary>
    public string? Biography { get; set; }

    /// <summary>FK to profile photo attachment.</summary>
    public Guid? PhotoId { get; set; }

    // Navigation properties
    public Attachment? Photo { get; set; }
    public ICollection<MangaSeries> AuthoredSeries { get; set; } = [];
    public ICollection<MangaSeries> IllustratedSeries { get; set; } = [];
}
