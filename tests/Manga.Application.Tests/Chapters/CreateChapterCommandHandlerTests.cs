using Manga.Application.Chapters.Commands.CreateChapter;
using Manga.Application.Common.Services;
using Manga.Application.Tests.Auth;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using NSubstitute;

namespace Manga.Application.Tests.Chapters;

public class CreateChapterCommandHandlerTests
{
    private readonly IUserAuthorizationService _authService = Substitute.For<IUserAuthorizationService>();

    public CreateChapterCommandHandlerTests()
    {
        _authService.HasModeratorPermissionAsync(Arg.Any<CancellationToken>()).Returns(true);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesChapter()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id };
        db.MangaSeries.Add(manga);
        var image = new Attachment { FileName = "page1.jpg", StoragePath = "/p", Url = "https://example.com/p1.jpg", ContentType = "image/jpeg" };
        db.Attachments.Add(image);
        await db.SaveChangesAsync();

        var handler = new CreateChapterCommandHandler(db, _authService);
        var command = new CreateChapterCommand(manga.Id, 1, "Chapter 1", DateTimeOffset.UtcNow, [image.Id]);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        var chapter = db.Chapters.First();
        Assert.Equal(1, chapter.ChapterNumber);
        Assert.Equal("Chapter 1", chapter.Title);
        Assert.Equal("chapter-1", chapter.Slug);
        Assert.Equal(1, chapter.Pages);
    }

    [Fact]
    public async Task Handle_UpdatesDenormalizedCounts()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id, TotalChapters = 0, LatestChapterNumber = 0 };
        db.MangaSeries.Add(manga);
        var image = new Attachment { FileName = "p.jpg", StoragePath = "/p", Url = "https://example.com/p.jpg", ContentType = "image/jpeg" };
        db.Attachments.Add(image);
        await db.SaveChangesAsync();

        var handler = new CreateChapterCommandHandler(db, _authService);
        await handler.Handle(new CreateChapterCommand(manga.Id, 5, null, DateTimeOffset.UtcNow, [image.Id]), CancellationToken.None);

        var updated = db.MangaSeries.First();
        Assert.Equal(1, updated.TotalChapters);
        Assert.Equal(5, updated.LatestChapterNumber);
    }

    [Fact]
    public async Task Handle_DecimalChapter_CeilsLatestNumber()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id };
        db.MangaSeries.Add(manga);
        var image = new Attachment { FileName = "p.jpg", StoragePath = "/p", Url = "https://example.com/p.jpg", ContentType = "image/jpeg" };
        db.Attachments.Add(image);
        await db.SaveChangesAsync();

        var handler = new CreateChapterCommandHandler(db, _authService);
        await handler.Handle(new CreateChapterCommand(manga.Id, 10.5m, null, DateTimeOffset.UtcNow, [image.Id]), CancellationToken.None);

        Assert.Equal(11, db.MangaSeries.First().LatestChapterNumber);
    }

    [Fact]
    public async Task Handle_MangaNotFound_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new CreateChapterCommandHandler(db, _authService);

        var result = await handler.Handle(
            new CreateChapterCommand(Guid.NewGuid(), 1, null, DateTimeOffset.UtcNow, [Guid.NewGuid()]),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Manga series not found", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_DuplicateChapterNumber_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id };
        db.MangaSeries.Add(manga);
        db.Chapters.Add(new Chapter { MangaSeriesId = manga.Id, ChapterNumber = 1, Slug = "ch-1", PublishedAt = DateTimeOffset.UtcNow });
        var image = new Attachment { FileName = "p.jpg", StoragePath = "/p", Url = "https://example.com/p.jpg", ContentType = "image/jpeg" };
        db.Attachments.Add(image);
        await db.SaveChangesAsync();

        var handler = new CreateChapterCommandHandler(db, _authService);
        var result = await handler.Handle(
            new CreateChapterCommand(manga.Id, 1, null, DateTimeOffset.UtcNow, [image.Id]),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("already exists", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_NonOwnerNonModerator_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id, CreatedBy = "user-1" };
        db.MangaSeries.Add(manga);
        await db.SaveChangesAsync();

        _authService.HasModeratorPermissionAsync(Arg.Any<CancellationToken>()).Returns(false);
        _authService.IsOwner("user-1").Returns(false);

        var handler = new CreateChapterCommandHandler(db, _authService);
        var result = await handler.Handle(
            new CreateChapterCommand(manga.Id, 1, null, DateTimeOffset.UtcNow, [Guid.NewGuid()]),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("your own manga", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_InvalidPageImageIds_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id };
        db.MangaSeries.Add(manga);
        await db.SaveChangesAsync();

        var handler = new CreateChapterCommandHandler(db, _authService);
        var result = await handler.Handle(
            new CreateChapterCommand(manga.Id, 1, null, DateTimeOffset.UtcNow, [Guid.NewGuid()]),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("page images not found", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_CreatesChapterPages()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id };
        db.MangaSeries.Add(manga);
        var img1 = new Attachment { FileName = "p1.jpg", StoragePath = "/p", Url = "https://example.com/p1.jpg", ContentType = "image/jpeg" };
        var img2 = new Attachment { FileName = "p2.jpg", StoragePath = "/p", Url = "https://example.com/p2.jpg", ContentType = "image/jpeg" };
        db.Attachments.AddRange(img1, img2);
        await db.SaveChangesAsync();

        var handler = new CreateChapterCommandHandler(db, _authService);
        var result = await handler.Handle(
            new CreateChapterCommand(manga.Id, 1, null, DateTimeOffset.UtcNow, [img1.Id, img2.Id]),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        var pages = db.ChapterPages.Where(p => p.ChapterId == result.Value).OrderBy(p => p.PageNumber).ToList();
        Assert.Equal(2, pages.Count);
        Assert.Equal(1, pages[0].PageNumber);
        Assert.Equal(img1.Id, pages[0].ImageId);
        Assert.Equal(2, pages[1].PageNumber);
        Assert.Equal(img2.Id, pages[1].ImageId);
    }
}
