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

    /// <summary>Grid size for chapter page scrambling (e.g. 8 = 8x8 = 64 tiles). Valid range: 2-64.</summary>
    private int _scrambleGridSize = 8;
    public int ScrambleGridSize
    {
        get => _scrambleGridSize;
        set => _scrambleGridSize = value is >= 2 and <= 64
            ? value
            : throw new ArgumentOutOfRangeException(nameof(ScrambleGridSize), "Must be between 2 and 64.");
    }
}
