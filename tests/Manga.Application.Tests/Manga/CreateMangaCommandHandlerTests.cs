using Manga.Application.Manga.Commands.CreateManga;
using Manga.Application.Tests.Auth;
using Manga.Domain.Entities;
using Manga.Domain.Enums;

namespace Manga.Application.Tests.Manga;

public class CreateMangaCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_CreatesMangaAndReturnsId()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        var genre = new Genre { Name = "Action", Slug = "action" };
        db.Persons.Add(author);
        db.Genres.Add(genre);
        await db.SaveChangesAsync();

        var handler = new CreateMangaCommandHandler(db);
        var command = new CreateMangaCommand(
            "Test Manga", "Synopsis", author.Id, null,
            [genre.Id], SeriesStatus.Ongoing, 2024, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        var manga = db.MangaSeries.FirstOrDefault(m => m.Id == result.Value);
        Assert.NotNull(manga);
        Assert.Equal("Test Manga", manga.Title);
        Assert.Equal(author.Id, manga.AuthorId);
    }

    [Fact]
    public async Task Handle_WithInvalidAuthorId_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var genre = new Genre { Name = "Action", Slug = "action" };
        db.Genres.Add(genre);
        await db.SaveChangesAsync();

        var handler = new CreateMangaCommandHandler(db);
        var command = new CreateMangaCommand(
            "Test Manga", null, Guid.NewGuid(), null,
            [genre.Id], SeriesStatus.Ongoing, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Author not found", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_WithInvalidArtistId_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        var genre = new Genre { Name = "Action", Slug = "action" };
        db.Persons.Add(author);
        db.Genres.Add(genre);
        await db.SaveChangesAsync();

        var handler = new CreateMangaCommandHandler(db);
        var command = new CreateMangaCommand(
            "Test Manga", null, author.Id, Guid.NewGuid(),
            [genre.Id], SeriesStatus.Ongoing, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Artist not found", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_WithInvalidGenreIds_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        await db.SaveChangesAsync();

        var handler = new CreateMangaCommandHandler(db);
        var command = new CreateMangaCommand(
            "Test Manga", null, author.Id, null,
            [Guid.NewGuid()], SeriesStatus.Ongoing, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("genres not found", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_WithInvalidCoverId_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        var genre = new Genre { Name = "Action", Slug = "action" };
        db.Persons.Add(author);
        db.Genres.Add(genre);
        await db.SaveChangesAsync();

        var handler = new CreateMangaCommandHandler(db);
        var command = new CreateMangaCommand(
            "Test Manga", null, author.Id, null,
            [genre.Id], SeriesStatus.Ongoing, null, Guid.NewGuid(), null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Cover attachment not found", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_CreatesGenreAssociations()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        var genre1 = new Genre { Name = "Action", Slug = "action" };
        var genre2 = new Genre { Name = "Comedy", Slug = "comedy" };
        db.Persons.Add(author);
        db.Genres.AddRange(genre1, genre2);
        await db.SaveChangesAsync();

        var handler = new CreateMangaCommandHandler(db);
        var command = new CreateMangaCommand(
            "Test Manga", null, author.Id, null,
            [genre1.Id, genre2.Id], SeriesStatus.Ongoing, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        var mangaGenres = db.MangaGenres.Where(mg => mg.MangaSeriesId == result.Value).ToList();
        Assert.Equal(2, mangaGenres.Count);
    }

    [Fact]
    public async Task Handle_WithValidCoverAndBanner_SetsForeignKeys()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        var genre = new Genre { Name = "Action", Slug = "action" };
        var cover = new Attachment { FileName = "cover.jpg", StoragePath = "/covers", Url = "https://example.com/cover.jpg", ContentType = "image/jpeg" };
        var banner = new Attachment { FileName = "banner.jpg", StoragePath = "/banners", Url = "https://example.com/banner.jpg", ContentType = "image/jpeg" };
        db.Persons.Add(author);
        db.Genres.Add(genre);
        db.Attachments.AddRange(cover, banner);
        await db.SaveChangesAsync();

        var handler = new CreateMangaCommandHandler(db);
        var command = new CreateMangaCommand(
            "Test Manga", null, author.Id, null,
            [genre.Id], SeriesStatus.Ongoing, null, cover.Id, banner.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        var manga = db.MangaSeries.First(m => m.Id == result.Value);
        Assert.Equal(cover.Id, manga.CoverId);
        Assert.Equal(banner.Id, manga.BannerId);
    }
}
