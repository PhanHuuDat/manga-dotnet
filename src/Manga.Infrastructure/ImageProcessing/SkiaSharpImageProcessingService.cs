using Manga.Application.Common.Interfaces;
using Manga.Domain.Enums;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace Manga.Infrastructure.ImageProcessing;

/// <summary>
/// Image processing using SkiaSharp: resize, WebP conversion, thumbnail generation.
/// </summary>
public class SkiaSharpImageProcessingService(
    IOptions<ImageProcessingSettings> options) : IImageProcessingService
{
    private readonly ImageProcessingSettings _settings = options.Value;

    public Task<ProcessedImageResult> ProcessAsync(
        Stream inputStream, AttachmentType type, CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            using var original = SKBitmap.Decode(inputStream)
                ?? throw new InvalidOperationException("Failed to decode image.");

            // Guard against extremely large images (max 8192x8192)
            if (original.Width > 8192 || original.Height > 8192)
                throw new InvalidOperationException("Image dimensions exceed maximum allowed (8192x8192).");

            var (maxW, maxH, crop) = GetProcessedDimensions(type);
            using var processed = ResizeImage(original, maxW, maxH, crop);
            var processedStream = EncodeToWebP(processed, _settings.WebPQuality);

            MemoryStream? thumbnailStream = null;
            var thumbDims = GetThumbnailDimensions(type);
            if (thumbDims is not null)
            {
                var (thumbW, thumbH) = thumbDims.Value;
                using var thumbnail = ResizeImage(original, thumbW, thumbH, false);
                thumbnailStream = EncodeToWebP(thumbnail, _settings.ThumbnailQuality);
            }

            return new ProcessedImageResult(processedStream, "image/webp", thumbnailStream);
        }, ct);
    }

    private static SKBitmap ResizeImage(SKBitmap source, int maxWidth, int maxHeight, bool crop)
    {
        if (crop)
            return CenterCrop(source, maxWidth, maxHeight);

        var (newW, newH) = CalculateFitDimensions(
            source.Width, source.Height, maxWidth, maxHeight);

        // Don't upscale
        if (newW >= source.Width && newH >= source.Height)
        {
            var copy = new SKBitmap(source.Width, source.Height);
            using var canvas = new SKCanvas(copy);
            canvas.DrawBitmap(source, 0, 0);
            return copy;
        }

        var resized = new SKBitmap(newW, newH);
        using var resizeCanvas = new SKCanvas(resized);
        using var paint = new SKPaint();
        resizeCanvas.DrawBitmap(source,
            new SKRect(0, 0, newW, newH), paint);
        return resized;
    }

    private static SKBitmap CenterCrop(SKBitmap source, int targetWidth, int targetHeight)
    {
        // Don't upscale the crop target
        var cropW = Math.Min(targetWidth, source.Width);
        var cropH = Math.Min(targetHeight, source.Height);

        // Calculate crop region (center)
        var sourceAspect = (float)source.Width / source.Height;
        var targetAspect = (float)cropW / cropH;

        int srcX, srcY, srcW, srcH;
        if (sourceAspect > targetAspect)
        {
            srcH = source.Height;
            srcW = (int)(srcH * targetAspect);
            srcX = (source.Width - srcW) / 2;
            srcY = 0;
        }
        else
        {
            srcW = source.Width;
            srcH = (int)(srcW / targetAspect);
            srcX = 0;
            srcY = (source.Height - srcH) / 2;
        }

        var cropped = new SKBitmap(cropW, cropH);
        using var canvas = new SKCanvas(cropped);
        using var paint = new SKPaint();
        canvas.DrawBitmap(source,
            new SKRect(srcX, srcY, srcX + srcW, srcY + srcH),
            new SKRect(0, 0, cropW, cropH),
            paint);
        return cropped;
    }

    private static (int Width, int Height) CalculateFitDimensions(
        int sourceW, int sourceH, int maxW, int maxH)
    {
        // 0 means no constraint on that dimension
        if (maxW <= 0 && maxH <= 0)
            return (sourceW, sourceH);

        var ratioW = maxW > 0 ? (float)maxW / sourceW : float.MaxValue;
        var ratioH = maxH > 0 ? (float)maxH / sourceH : float.MaxValue;
        var ratio = Math.Min(ratioW, ratioH);

        // Don't upscale
        if (ratio >= 1.0f)
            return (sourceW, sourceH);

        return (Math.Max(1, (int)(sourceW * ratio)),
                Math.Max(1, (int)(sourceH * ratio)));
    }

    private static MemoryStream EncodeToWebP(SKBitmap bitmap, int quality)
    {
        using var image = SKImage.FromBitmap(bitmap);
        var data = image.Encode(SKEncodedImageFormat.Webp, quality)
            ?? throw new InvalidOperationException("Failed to encode image to WebP.");
        var ms = new MemoryStream();
        data.SaveTo(ms);
        ms.Position = 0;
        return ms;
    }

    private static (int MaxWidth, int MaxHeight, bool Crop) GetProcessedDimensions(
        AttachmentType type) => type switch
    {
        AttachmentType.MangaCover => (300, 450, false),
        AttachmentType.MangaBanner => (1200, 400, false),
        AttachmentType.ChapterPage => (1920, 0, false),
        AttachmentType.Avatar => (200, 200, true),
        AttachmentType.PersonPhoto => (300, 300, true),
        _ => (1920, 1080, false),
    };

    private static (int MaxWidth, int MaxHeight)? GetThumbnailDimensions(
        AttachmentType type) => type switch
    {
        AttachmentType.MangaCover => (150, 225),
        AttachmentType.MangaBanner => (600, 200),
        AttachmentType.ChapterPage => (400, 0),
        _ => null,
    };
}
