namespace Manga.Infrastructure.ImageProcessing;

/// <summary>
/// Configuration for image processing quality settings.
/// </summary>
public class ImageProcessingSettings
{
    public const string SectionName = "ImageProcessing";

    /// <summary>WebP quality for processed images (0-100).</summary>
    public int WebPQuality { get; set; } = 85;

    /// <summary>WebP quality for thumbnails (0-100).</summary>
    public int ThumbnailQuality { get; set; } = 75;
}
