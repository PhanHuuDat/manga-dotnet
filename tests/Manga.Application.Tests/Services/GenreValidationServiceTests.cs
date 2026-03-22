using Manga.Application.Common.Models;
using Manga.Application.Common.Services;
using Manga.Application.Tests.Auth;
using Manga.Domain.Entities;

namespace Manga.Application.Tests.Services;

public class GenreValidationServiceTests
{
    [Fact]
    public async Task ValidateAllExistAsync_WithEmptyGenreIds_ReturnsSuccess()
    {
        using var db = TestDbContextFactory.Create();
        var service = new GenreValidationService(db);

        var result = await service.ValidateAllExistAsync([], CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateAllExistAsync_WithSingleExistingGenre_ReturnsSuccess()
    {
        using var db = TestDbContextFactory.Create();
        var genre = new Genre { Name = "Action", Slug = "action" };
        db.Genres.Add(genre);
        await db.SaveChangesAsync();

        var service = new GenreValidationService(db);
        var result = await service.ValidateAllExistAsync([genre.Id], CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateAllExistAsync_WithMultipleExistingGenres_ReturnsSuccess()
    {
        using var db = TestDbContextFactory.Create();
        var genre1 = new Genre { Name = "Action", Slug = "action" };
        var genre2 = new Genre { Name = "Comedy", Slug = "comedy" };
        var genre3 = new Genre { Name = "Drama", Slug = "drama" };
        db.Genres.AddRange(genre1, genre2, genre3);
        await db.SaveChangesAsync();

        var service = new GenreValidationService(db);
        var result = await service.ValidateAllExistAsync([genre1.Id, genre2.Id, genre3.Id], CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateAllExistAsync_WithNonexistentGenre_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var service = new GenreValidationService(db);

        var result = await service.ValidateAllExistAsync([Guid.NewGuid()], CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Contains("genres not found", result.Errors[0]);
    }

    [Fact]
    public async Task ValidateAllExistAsync_WithMixedExistingAndNonexistent_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var genre = new Genre { Name = "Action", Slug = "action" };
        db.Genres.Add(genre);
        await db.SaveChangesAsync();

        var service = new GenreValidationService(db);
        var result = await service.ValidateAllExistAsync([genre.Id, Guid.NewGuid()], CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Contains("genres not found", result.Errors[0]);
    }

    [Fact]
    public async Task ValidateAllExistAsync_RequiresAllGenresToExist()
    {
        using var db = TestDbContextFactory.Create();
        var genre1 = new Genre { Name = "Action", Slug = "action" };
        var genre2 = new Genre { Name = "Comedy", Slug = "comedy" };
        db.Genres.AddRange(genre1, genre2);
        await db.SaveChangesAsync();

        var service = new GenreValidationService(db);
        // Request 3 genres but only 2 exist
        var result = await service.ValidateAllExistAsync([genre1.Id, genre2.Id, Guid.NewGuid()], CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ValidateAllExistAsync_CancellationTokenIsRespected()
    {
        using var db = TestDbContextFactory.Create();
        var genre = new Genre { Name = "Action", Slug = "action" };
        db.Genres.Add(genre);
        await db.SaveChangesAsync();

        var service = new GenreValidationService(db);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.ValidateAllExistAsync([genre.Id], cts.Token));
    }
}
