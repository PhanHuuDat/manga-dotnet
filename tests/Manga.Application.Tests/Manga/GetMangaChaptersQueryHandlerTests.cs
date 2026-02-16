using Manga.Application.Manga.Queries.GetMangaChapters;
using Manga.Application.Tests.Auth;
using Manga.Domain.Entities;

namespace Manga.Application.Tests.Manga;

public class GetMangaChaptersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsChaptersOrderedByNumber()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id };
        db.MangaSeries.Add(manga);
        db.Chapters.Add(new Chapter { MangaSeriesId = manga.Id, ChapterNumber = 2, Slug = "ch-2", PublishedAt = DateTimeOffset.UtcNow });
        db.Chapters.Add(new Chapter { MangaSeriesId = manga.Id, ChapterNumber = 1, Slug = "ch-1", PublishedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var handler = new GetMangaChaptersQueryHandler(db);
        var result = await handler.Handle(new GetMangaChaptersQuery(manga.Id, 1, 50), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value!.Data.Count);
        Assert.Equal(1, result.Value.Data[0].ChapterNumber);
        Assert.Equal(2, result.Value.Data[1].ChapterNumber);
    }

    [Fact]
    public async Task Handle_MangaNotFound_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetMangaChaptersQueryHandler(db);

        var result = await handler.Handle(new GetMangaChaptersQuery(Guid.NewGuid(), 1, 50), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Manga not found", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_PaginatesCorrectly()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id };
        db.MangaSeries.Add(manga);
        for (var i = 1; i <= 5; i++)
            db.Chapters.Add(new Chapter { MangaSeriesId = manga.Id, ChapterNumber = i, Slug = $"ch-{i}", PublishedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var handler = new GetMangaChaptersQueryHandler(db);
        var result = await handler.Handle(new GetMangaChaptersQuery(manga.Id, 1, 2), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Value!.Data.Count);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.True(result.Value.HasNext);
    }
}
