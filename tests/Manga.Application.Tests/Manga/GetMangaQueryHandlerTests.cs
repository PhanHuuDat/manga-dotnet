using Manga.Application.Manga.Queries.GetManga;
using Manga.Application.Tests.Auth;
using Manga.Domain.Entities;
using Manga.Domain.Enums;

namespace Manga.Application.Tests.Manga;

public class GetMangaQueryHandlerTests
{
    [Fact]
    public async Task Handle_ExistingManga_ReturnsDetailDto()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author Name" };
        var genre = new Genre { Name = "Action", Slug = "action" };
        db.Persons.Add(author);
        db.Genres.Add(genre);
        var manga = new MangaSeries
        {
            Title = "Test Manga",
            Synopsis = "A synopsis",
            AuthorId = author.Id,
            Status = SeriesStatus.Ongoing,
            Rating = 4.5m,
            Views = 1000,
            TotalChapters = 10,
        };
        db.MangaSeries.Add(manga);
        db.MangaGenres.Add(new MangaGenre { MangaSeriesId = manga.Id, GenreId = genre.Id });
        db.AlternativeTitles.Add(new AlternativeTitle { MangaSeriesId = manga.Id, Title = "Alt Title" });
        await db.SaveChangesAsync();

        var handler = new GetMangaQueryHandler(db);
        var result = await handler.Handle(new GetMangaQuery(manga.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        var dto = result.Value!;
        Assert.Equal("Test Manga", dto.Title);
        Assert.Equal("A synopsis", dto.Synopsis);
        Assert.Equal("Author Name", dto.Author.Name);
        Assert.Single(dto.Genres);
        Assert.Single(dto.AlternativeTitles);
        Assert.Equal("Alt Title", dto.AlternativeTitles[0]);
    }

    [Fact]
    public async Task Handle_NonExistentManga_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetMangaQueryHandler(db);

        var result = await handler.Handle(new GetMangaQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Manga not found", result.Errors[0]);
    }
}
