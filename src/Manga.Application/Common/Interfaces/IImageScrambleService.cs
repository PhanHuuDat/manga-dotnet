namespace Manga.Application.Common.Interfaces;

/// <summary>
/// Tile-based image scrambling for anti-leak protection.
/// Grid size is read from configuration internally.
/// </summary>
public interface IImageScrambleService
{
    /// <summary>
    /// Scramble a WebP image stream by shuffling NxN tiles using Fisher-Yates.
    /// Returns scrambled stream, seed, and grid size used.
    /// </summary>
    Task<ScrambleResult> ScrambleAsync(
        Stream imageStream,
        CancellationToken ct = default);
}

/// <summary>
/// Result of image scrambling containing the scrambled stream and metadata.
/// </summary>
public record ScrambleResult(
    MemoryStream ScrambledStream,
    int Seed,
    int GridSize) : IDisposable
{
    public void Dispose() => ScrambledStream.Dispose();
}
