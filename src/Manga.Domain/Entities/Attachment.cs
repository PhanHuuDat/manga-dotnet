using Manga.Domain.Common;
using Manga.Domain.Enums;

namespace Manga.Domain.Entities;

/// <summary>
/// Centralized file/image storage record with type classification.
/// </summary>
public class Attachment : AuditableEntity
{
    /// <summary>Original file name as uploaded.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Internal storage path (e.g. S3 key or local path).</summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>Public-facing URL to access the file.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>MIME content type (e.g. image/jpeg).</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>File size in bytes.</summary>
    public long FileSize { get; set; }

    /// <summary>Classification of the attachment usage.</summary>
    public AttachmentType Type { get; set; }

    /// <summary>Public URL for the thumbnail version (covers, banners, chapter pages).</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>Internal storage path for thumbnail.</summary>
    public string? ThumbnailStoragePath { get; set; }

    /// <summary>
    /// Fisher-Yates shuffle seed used to scramble this image.
    /// Null means the image is not scrambled (legacy or non-chapter-page).
    /// </summary>
    public int? ScrambleSeed { get; set; }

    /// <summary>
    /// Grid dimension used for tile scrambling (e.g. 8 means 8x8 = 64 tiles).
    /// Null means the image is not scrambled.
    /// </summary>
    public int? ScrambleGridSize { get; set; }
}
