using Manga.Infrastructure.ImageProcessing;

namespace Manga.Infrastructure.Tests.ImageProcessing;

public class ImageScrambleServiceTests
{
    [Fact]
    public void Mulberry32_SameSeed_ProducesDeterministicSequence()
    {
        var seq1 = GetPermutation(42, 10);
        var seq2 = GetPermutation(42, 10);

        Assert.Equal(seq1, seq2);
    }

    [Fact]
    public void Mulberry32_DifferentSeeds_ProduceDifferentSequences()
    {
        var seq1 = GetPermutation(42, 10);
        var seq2 = GetPermutation(99, 10);

        Assert.NotEqual(seq1, seq2);
    }

    [Fact]
    public void GeneratePermutation_IsValidPermutation()
    {
        var perm = SkiaSharpImageScrambleService.GeneratePermutation(64, 12345);

        Assert.Equal(64, perm.Length);
        var sorted = perm.OrderBy(x => x).ToArray();
        for (var i = 0; i < 64; i++)
            Assert.Equal(i, sorted[i]);
    }

    [Fact]
    public void GeneratePermutation_IsShuffled_NotIdentity()
    {
        var perm = SkiaSharpImageScrambleService.GeneratePermutation(64, 12345);

        var displacedCount = perm.Where((val, idx) => val != idx).Count();
        Assert.True(displacedCount > 10, "Permutation should shuffle most elements");
    }

    [Fact]
    public void GeneratePermutation_Deterministic_SameSeedSameResult()
    {
        var perm1 = SkiaSharpImageScrambleService.GeneratePermutation(64, 999);
        var perm2 = SkiaSharpImageScrambleService.GeneratePermutation(64, 999);

        Assert.Equal(perm1, perm2);
    }

    [Fact]
    public void GetTileRect_UniformDimensions_EqualTiles()
    {
        // 1600x2400 with 8x8 grid: 200x300 per tile
        var rect = SkiaSharpImageScrambleService.GetTileRect(1600, 2400, 8, 0);
        Assert.Equal(0, rect.Left);
        Assert.Equal(0, rect.Top);
        Assert.Equal(200, rect.Right);
        Assert.Equal(300, rect.Bottom);
    }

    [Fact]
    public void GetTileRect_LastTile_AbsorbsRemainder()
    {
        // 1700x2400 with 8x8: baseTileW = 212 (1700/8)
        // Last col (7): left=7*212=1484, right=1700
        var lastColRect = SkiaSharpImageScrambleService.GetTileRect(1700, 2400, 8, 7);
        Assert.Equal(1484, lastColRect.Left);
        Assert.Equal(1700, lastColRect.Right);
        Assert.Equal(216, lastColRect.Right - lastColRect.Left);
    }

    [Fact]
    public void GetTileRect_AllTiles_CoverFullImage()
    {
        const int w = 1920;
        const int h = 2400;
        const int grid = 8;

        // Verify tiles cover entire width and height without gaps
        for (var row = 0; row < grid; row++)
        {
            var prevRight = 0;
            for (var col = 0; col < grid; col++)
            {
                var idx = row * grid + col;
                var rect = SkiaSharpImageScrambleService.GetTileRect(w, h, grid, idx);
                Assert.Equal(prevRight, rect.Left);
                prevRight = rect.Right;
            }
            Assert.Equal(w, prevRight);
        }

        for (var col = 0; col < grid; col++)
        {
            var prevBottom = 0;
            for (var row = 0; row < grid; row++)
            {
                var idx = row * grid + col;
                var rect = SkiaSharpImageScrambleService.GetTileRect(w, h, grid, idx);
                Assert.Equal(prevBottom, rect.Top);
                prevBottom = rect.Bottom;
            }
            Assert.Equal(h, prevBottom);
        }
    }

    /// <summary>
    /// CROSS-PLATFORM PARITY: Known permutation for seed 12345, 16 elements.
    /// The TypeScript test MUST produce this exact same array.
    /// </summary>
    [Fact]
    public void GeneratePermutation_KnownSeed_MatchesCrossPlatformBaseline()
    {
        var perm = SkiaSharpImageScrambleService.GeneratePermutation(16, 12345);

        int[] expected = [11, 7, 14, 12, 1, 4, 2, 15, 10, 13, 3, 6, 8, 0, 9, 5];
        Assert.Equal(expected, perm);
    }

    private static int[] GetPermutation(int seed, int count)
    {
        return SkiaSharpImageScrambleService.GeneratePermutation(count, seed);
    }
}
