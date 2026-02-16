using Manga.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace Manga.Infrastructure.ImageProcessing;

/// <summary>
/// SkiaSharp implementation of tile-based image scrambling.
/// Uses Fisher-Yates shuffle with deterministic mulberry32 PRNG
/// for cross-platform parity with the TypeScript frontend.
/// </summary>
public class SkiaSharpImageScrambleService(
    IOptions<ImageProcessingSettings> options) : IImageScrambleService
{
    private readonly int _gridSize = options.Value.ScrambleGridSize;

    public Task<ScrambleResult> ScrambleAsync(
        Stream imageStream, CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            using var original = SKBitmap.Decode(imageStream)
                ?? throw new InvalidOperationException("Failed to decode image for scrambling.");

            // Skip scrambling if image is too small for the grid
            if (original.Width < _gridSize || original.Height < _gridSize)
                throw new InvalidOperationException(
                    $"Image dimensions ({original.Width}x{original.Height}) too small for {_gridSize}x{_gridSize} grid scrambling.");

            var seed = Random.Shared.Next();

            using var scrambled = ScrambleBitmap(original, _gridSize, seed);

            // Encode to lossless WebP (quality 100 = lossless in SkiaSharp)
            using var image = SKImage.FromBitmap(scrambled);
            var data = image.Encode(SKEncodedImageFormat.Webp, 100)
                ?? throw new InvalidOperationException("Failed to encode scrambled image.");

            var ms = new MemoryStream();
            data.SaveTo(ms);
            ms.Position = 0;

            return new ScrambleResult(ms, seed, _gridSize);
        }, ct);
    }

    /// <summary>
    /// Divide bitmap into gridSize x gridSize tiles and shuffle using Fisher-Yates.
    /// Handles non-uniform tile sizes when dimensions don't divide evenly.
    /// </summary>
    internal static SKBitmap ScrambleBitmap(SKBitmap source, int gridSize, int seed)
    {
        var totalTiles = gridSize * gridSize;
        var permutation = GeneratePermutation(totalTiles, seed);

        var output = new SKBitmap(source.Width, source.Height);
        using var canvas = new SKCanvas(output);

        for (var dstIdx = 0; dstIdx < totalTiles; dstIdx++)
        {
            var srcIdx = permutation[dstIdx];

            var srcRect = GetTileRect(source.Width, source.Height, gridSize, srcIdx);
            var dstRect = GetTileRect(source.Width, source.Height, gridSize, dstIdx);

            using var tile = new SKBitmap();
            source.ExtractSubset(tile, srcRect);

            canvas.DrawBitmap(tile, new SKRect(dstRect.Left, dstRect.Top, dstRect.Right, dstRect.Bottom));
        }

        canvas.Flush();
        return output;
    }

    /// <summary>
    /// Get pixel rect for tile at given index in the grid.
    /// Last row/column tiles absorb remainder pixels.
    /// </summary>
    internal static SKRectI GetTileRect(int imageW, int imageH, int gridSize, int tileIndex)
    {
        var col = tileIndex % gridSize;
        var row = tileIndex / gridSize;

        var baseTileW = imageW / gridSize;
        var baseTileH = imageH / gridSize;

        var left = col * baseTileW;
        var top = row * baseTileH;

        var right = (col == gridSize - 1) ? imageW : left + baseTileW;
        var bottom = (row == gridSize - 1) ? imageH : top + baseTileH;

        return new SKRectI(left, top, right, bottom);
    }

    /// <summary>
    /// Fisher-Yates shuffle with mulberry32 PRNG for cross-platform determinism.
    /// permutation[dstIdx] = srcIdx means "tile at srcIdx goes to dstIdx".
    /// </summary>
    internal static int[] GeneratePermutation(int count, int seed)
    {
        var perm = new int[count];
        for (var i = 0; i < count; i++)
            perm[i] = i;

        var s = (uint)seed;
        for (var i = count - 1; i > 0; i--)
        {
            var rnd = Mulberry32(ref s);
            var j = (int)(rnd % (uint)(i + 1));
            (perm[i], perm[j]) = (perm[j], perm[i]);
        }

        return perm;
    }

    /// <summary>
    /// Mulberry32 PRNG -- identical implementation exists in TypeScript.
    /// Must NOT be changed without updating the frontend counterpart.
    /// </summary>
    internal static uint Mulberry32(ref uint seed)
    {
        seed += 0x6D2B79F5;
        var t = seed;
        t = (t ^ (t >> 15)) * (1 | t);
        t = (t + (t ^ (t >> 7)) * (61 | t)) ^ t;
        return t ^ (t >> 14);
    }
}
