using Manga.Domain.Enums;

namespace Manga.Application.Common.Interfaces;

/// <summary>
/// Processes images: resize, convert to WebP, generate thumbnails.
/// </summary>
public interface IImageProcessingService
{
    /// <summary>
    /// Process an image according to its attachment type.
    /// Returns processed image stream and optional thumbnail stream.
    /// </summary>
    Task<ProcessedImageResult> ProcessAsync(
        Stream inputStream,
        AttachmentType type,
        CancellationToken ct = default);
}

/// <summary>
/// Result of image processing containing processed and optional thumbnail streams.
/// </summary>
public record ProcessedImageResult(
    MemoryStream ProcessedStream,
    string ProcessedContentType,
    MemoryStream? ThumbnailStream) : IDisposable
{
    public void Dispose()
    {
        ProcessedStream.Dispose();
        ThumbnailStream?.Dispose();
    }
}
