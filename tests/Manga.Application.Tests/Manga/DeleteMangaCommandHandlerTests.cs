using Manga.Application.Manga.Commands.DeleteManga;
using Manga.Application.Tests.Auth;
using Manga.Domain.Entities;

namespace Manga.Application.Tests.Manga;

public class DeleteMangaCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingManga_SoftDeletes()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Title", AuthorId = author.Id };
        db.MangaSeries.Add(manga);
        await db.SaveChangesAsync();

        var handler = new DeleteMangaCommandHandler(db);
        var result = await handler.Handle(new DeleteMangaCommand(manga.Id), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.True(manga.IsDeleted);
        Assert.NotNull(manga.DeletedAt);
    }

    [Fact]
    public async Task Handle_NonExistentManga_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new DeleteMangaCommandHandler(db);

        var result = await handler.Handle(new DeleteMangaCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Manga not found", result.Errors[0]);
    }
}
