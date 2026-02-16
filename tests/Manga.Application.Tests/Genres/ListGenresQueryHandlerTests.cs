using Manga.Application.Genres.Queries.ListGenres;
using Manga.Application.Tests.Auth;
using Manga.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Manga.Application.Tests.Genres;

public class ListGenresQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsGenresOrderedByName()
    {
        using var db = TestDbContextFactory.Create();
        db.Genres.Add(new Genre { Name = "Romance", Slug = "romance" });
        db.Genres.Add(new Genre { Name = "Action", Slug = "action" });
        await db.SaveChangesAsync();

        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new ListGenresQueryHandler(db, cache);

        var result = await handler.Handle(new ListGenresQuery(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal("Action", result.Value[0].Name);
        Assert.Equal("Romance", result.Value[1].Name);
    }

    [Fact]
    public async Task Handle_IncludesMangaCount()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var genre = new Genre { Name = "Action", Slug = "action" };
        db.Genres.Add(genre);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id };
        db.MangaSeries.Add(manga);
        db.MangaGenres.Add(new MangaGenre { MangaSeriesId = manga.Id, GenreId = genre.Id });
        await db.SaveChangesAsync();

        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new ListGenresQueryHandler(db, cache);

        var result = await handler.Handle(new ListGenresQuery(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Value![0].MangaCount);
    }

    [Fact]
    public async Task Handle_CachesResult()
    {
        using var db = TestDbContextFactory.Create();
        db.Genres.Add(new Genre { Name = "Action", Slug = "action" });
        await db.SaveChangesAsync();

        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new ListGenresQueryHandler(db, cache);

        // First call populates cache
        await handler.Handle(new ListGenresQuery(), CancellationToken.None);

        // Add another genre — should not appear due to cache
        db.Genres.Add(new Genre { Name = "Comedy", Slug = "comedy" });
        await db.SaveChangesAsync();

        var result = await handler.Handle(new ListGenresQuery(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(result.Value!); // Still 1 from cache
    }

    [Fact]
    public async Task Handle_EmptyGenres_ReturnsEmptyList()
    {
        using var db = TestDbContextFactory.Create();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new ListGenresQueryHandler(db, cache);

        var result = await handler.Handle(new ListGenresQuery(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Value!);
    }
}
