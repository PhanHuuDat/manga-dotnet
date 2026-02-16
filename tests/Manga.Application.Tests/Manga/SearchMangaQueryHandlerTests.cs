using Manga.Application.Manga.Queries.SearchManga;
using Manga.Application.Tests.Auth;
using Manga.Domain.Entities;

namespace Manga.Application.Tests.Manga;

public class SearchMangaQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShortSearchTerm_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new SearchMangaQueryHandler(db);

        var result = await handler.Handle(new SearchMangaQuery("a", 1, 20), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("at least 2 characters", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_EmptySearchTerm_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new SearchMangaQueryHandler(db);

        var result = await handler.Handle(new SearchMangaQuery("", 1, 20), CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Handle_MatchesByTitle()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        db.MangaSeries.Add(new MangaSeries { Title = "Naruto", AuthorId = author.Id });
        db.MangaSeries.Add(new MangaSeries { Title = "One Piece", AuthorId = author.Id });
        await db.SaveChangesAsync();

        var handler = new SearchMangaQueryHandler(db);
        var result = await handler.Handle(new SearchMangaQuery("naruto", 1, 20), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(result.Value!.Data);
        Assert.Equal("Naruto", result.Value.Data[0].Title);
    }

    [Fact]
    public async Task Handle_CaseInsensitiveSearch()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        db.MangaSeries.Add(new MangaSeries { Title = "NARUTO", AuthorId = author.Id });
        await db.SaveChangesAsync();

        var handler = new SearchMangaQueryHandler(db);
        var result = await handler.Handle(new SearchMangaQuery("naruto", 1, 20), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(result.Value!.Data);
    }

    [Fact]
    public async Task Handle_MatchesByAuthorName()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Eiichiro Oda" };
        db.Persons.Add(author);
        db.MangaSeries.Add(new MangaSeries { Title = "One Piece", AuthorId = author.Id });
        await db.SaveChangesAsync();

        var handler = new SearchMangaQueryHandler(db);
        var result = await handler.Handle(new SearchMangaQuery("oda", 1, 20), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(result.Value!.Data);
    }

    [Fact]
    public async Task Handle_NoResults_ReturnsEmptyPage()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new SearchMangaQueryHandler(db);

        var result = await handler.Handle(new SearchMangaQuery("nonexistent", 1, 20), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Empty(result.Value!.Data);
        Assert.Equal(0, result.Value.TotalCount);
    }
}
