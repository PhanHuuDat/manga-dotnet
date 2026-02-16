using Manga.Application.Chapters.Commands.DeleteChapter;
using Manga.Application.Tests.Auth;
using Manga.Domain.Entities;

namespace Manga.Application.Tests.Chapters;

public class DeleteChapterCommandHandlerTests
{
    [Fact]
    public async Task Handle_SoftDeletesChapter()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id, TotalChapters = 2, LatestChapterNumber = 2 };
        db.MangaSeries.Add(manga);
        var ch1 = new Chapter { MangaSeriesId = manga.Id, ChapterNumber = 1, Slug = "ch-1", PublishedAt = DateTimeOffset.UtcNow };
        var ch2 = new Chapter { MangaSeriesId = manga.Id, ChapterNumber = 2, Slug = "ch-2", PublishedAt = DateTimeOffset.UtcNow };
        db.Chapters.AddRange(ch1, ch2);
        await db.SaveChangesAsync();

        var handler = new DeleteChapterCommandHandler(db);
        var result = await handler.Handle(new DeleteChapterCommand(ch2.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(ch2.IsDeleted);
        Assert.NotNull(ch2.DeletedAt);
    }

    [Fact]
    public async Task Handle_DecrementsTotalChapters()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id, TotalChapters = 3 };
        db.MangaSeries.Add(manga);
        var chapter = new Chapter { MangaSeriesId = manga.Id, ChapterNumber = 1, Slug = "ch-1", PublishedAt = DateTimeOffset.UtcNow };
        db.Chapters.Add(chapter);
        await db.SaveChangesAsync();

        var handler = new DeleteChapterCommandHandler(db);
        await handler.Handle(new DeleteChapterCommand(chapter.Id), CancellationToken.None);

        Assert.Equal(2, manga.TotalChapters);
    }

    [Fact]
    public async Task Handle_RecalculatesLatestChapterNumber()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id, TotalChapters = 2, LatestChapterNumber = 10 };
        db.MangaSeries.Add(manga);
        var ch1 = new Chapter { MangaSeriesId = manga.Id, ChapterNumber = 5, Slug = "ch-5", PublishedAt = DateTimeOffset.UtcNow };
        var ch2 = new Chapter { MangaSeriesId = manga.Id, ChapterNumber = 10, Slug = "ch-10", PublishedAt = DateTimeOffset.UtcNow };
        db.Chapters.AddRange(ch1, ch2);
        await db.SaveChangesAsync();

        var handler = new DeleteChapterCommandHandler(db);
        await handler.Handle(new DeleteChapterCommand(ch2.Id), CancellationToken.None);

        Assert.Equal(5, manga.LatestChapterNumber);
    }

    [Fact]
    public async Task Handle_ChapterNotFound_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new DeleteChapterCommandHandler(db);

        var result = await handler.Handle(new DeleteChapterCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Chapter not found", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_TotalChaptersDoesNotGoBelowZero()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id, TotalChapters = 0 };
        db.MangaSeries.Add(manga);
        var chapter = new Chapter { MangaSeriesId = manga.Id, ChapterNumber = 1, Slug = "ch-1", PublishedAt = DateTimeOffset.UtcNow };
        db.Chapters.Add(chapter);
        await db.SaveChangesAsync();

        var handler = new DeleteChapterCommandHandler(db);
        await handler.Handle(new DeleteChapterCommand(chapter.Id), CancellationToken.None);

        Assert.Equal(0, manga.TotalChapters);
    }
}
