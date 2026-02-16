using Manga.Application.Chapters.Queries.GetChapter;
using Manga.Application.Tests.Auth;
using Manga.Domain.Entities;
using Manga.Domain.Enums;

namespace Manga.Application.Tests.Chapters;

public class GetChapterQueryHandlerTests
{
    [Fact]
    public async Task Handle_ExistingChapter_ReturnsDetailDto()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Test Manga", AuthorId = author.Id };
        db.MangaSeries.Add(manga);
        var image = new Attachment { FileName = "p1.jpg", StoragePath = "/p", Url = "https://example.com/p1.jpg", ContentType = "image/jpeg" };
        db.Attachments.Add(image);
        var chapter = new Chapter
        {
            MangaSeriesId = manga.Id,
            ChapterNumber = 1,
            Title = "First Chapter",
            Slug = "chapter-1",
            Pages = 1,
            Views = 100,
            PublishedAt = DateTimeOffset.UtcNow,
        };
        db.Chapters.Add(chapter);
        db.ChapterPages.Add(new ChapterPage { ChapterId = chapter.Id, PageNumber = 1, ImageId = image.Id });
        await db.SaveChangesAsync();

        var handler = new GetChapterQueryHandler(db);
        var result = await handler.Handle(new GetChapterQuery(chapter.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        var dto = result.Value!;
        Assert.Equal("First Chapter", dto.Title);
        Assert.Equal("Test Manga", dto.MangaTitle);
        Assert.Equal(1, dto.ChapterNumber);
        Assert.Single(dto.Pages);
        Assert.Equal("https://example.com/p1.jpg", dto.Pages[0].ImageUrl);
    }

    [Fact]
    public async Task Handle_NonExistentChapter_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetChapterQueryHandler(db);

        var result = await handler.Handle(new GetChapterQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Chapter not found", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_PagesOrderedByNumber()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Manga", AuthorId = author.Id };
        db.MangaSeries.Add(manga);
        var img1 = new Attachment { FileName = "p1.jpg", StoragePath = "/p", Url = "https://example.com/p1.jpg", ContentType = "image/jpeg" };
        var img2 = new Attachment { FileName = "p2.jpg", StoragePath = "/p", Url = "https://example.com/p2.jpg", ContentType = "image/jpeg" };
        db.Attachments.AddRange(img1, img2);
        var chapter = new Chapter { MangaSeriesId = manga.Id, ChapterNumber = 1, Slug = "ch-1", Pages = 2, PublishedAt = DateTimeOffset.UtcNow };
        db.Chapters.Add(chapter);
        // Add in reverse order
        db.ChapterPages.Add(new ChapterPage { ChapterId = chapter.Id, PageNumber = 2, ImageId = img2.Id });
        db.ChapterPages.Add(new ChapterPage { ChapterId = chapter.Id, PageNumber = 1, ImageId = img1.Id });
        await db.SaveChangesAsync();

        var handler = new GetChapterQueryHandler(db);
        var result = await handler.Handle(new GetChapterQuery(chapter.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(1, result.Value!.Pages[0].PageNumber);
        Assert.Equal(2, result.Value.Pages[1].PageNumber);
    }
}
