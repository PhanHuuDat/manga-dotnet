using Manga.Application.Manga.Queries.ListManga;
using Manga.Application.Tests.Auth;
using Manga.Domain.Entities;
using Manga.Domain.Enums;

namespace Manga.Application.Tests.Manga;

public class ListMangaQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedManga()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        for (var i = 0; i < 5; i++)
            db.MangaSeries.Add(new MangaSeries { Title = $"Manga {i}", AuthorId = author.Id });
        await db.SaveChangesAsync();

        var handler = new ListMangaQueryHandler(db);
        var query = new ListMangaQuery(1, 3, null, null, MangaSortBy.Title);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Value!.Data.Count);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.True(result.Value.HasNext);
    }

    [Fact]
    public async Task Handle_FiltersByGenre()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        var genre = new Genre { Name = "Action", Slug = "action" };
        db.Persons.Add(author);
        db.Genres.Add(genre);
        var manga1 = new MangaSeries { Title = "With Genre", AuthorId = author.Id };
        var manga2 = new MangaSeries { Title = "Without Genre", AuthorId = author.Id };
        db.MangaSeries.AddRange(manga1, manga2);
        db.MangaGenres.Add(new MangaGenre { MangaSeriesId = manga1.Id, GenreId = genre.Id });
        await db.SaveChangesAsync();

        var handler = new ListMangaQueryHandler(db);
        var query = new ListMangaQuery(1, 20, genre.Id, null, MangaSortBy.Latest);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(result.Value!.Data);
        Assert.Equal("With Genre", result.Value.Data[0].Title);
    }

    [Fact]
    public async Task Handle_FiltersByStatus()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        db.MangaSeries.Add(new MangaSeries { Title = "Ongoing", AuthorId = author.Id, Status = SeriesStatus.Ongoing });
        db.MangaSeries.Add(new MangaSeries { Title = "Completed", AuthorId = author.Id, Status = SeriesStatus.Completed });
        await db.SaveChangesAsync();

        var handler = new ListMangaQueryHandler(db);
        var query = new ListMangaQuery(1, 20, null, SeriesStatus.Completed, MangaSortBy.Latest);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(result.Value!.Data);
        Assert.Equal("Completed", result.Value.Data[0].Title);
    }

    [Fact]
    public async Task Handle_ClampsPageSize()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        await db.SaveChangesAsync();

        var handler = new ListMangaQueryHandler(db);
        var query = new ListMangaQuery(1, 500, null, null, MangaSortBy.Latest);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(100, result.Value!.PageSize);
    }

    [Fact]
    public async Task Handle_SortsByRating()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        db.MangaSeries.Add(new MangaSeries { Title = "Low", AuthorId = author.Id, Rating = 2.0m });
        db.MangaSeries.Add(new MangaSeries { Title = "High", AuthorId = author.Id, Rating = 4.5m });
        await db.SaveChangesAsync();

        var handler = new ListMangaQueryHandler(db);
        var query = new ListMangaQuery(1, 20, null, null, MangaSortBy.Rating);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("High", result.Value!.Data[0].Title);
    }
}
