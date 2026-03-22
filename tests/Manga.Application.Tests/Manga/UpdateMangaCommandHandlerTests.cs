using Manga.Application.Common.Models;
using Manga.Application.Common.Services;
using Manga.Application.Manga.Commands.UpdateManga;
using Manga.Application.Tests.Auth;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using NSubstitute;

namespace Manga.Application.Tests.Manga;

public class UpdateMangaCommandHandlerTests
{
    private readonly IUserAuthorizationService _authService = Substitute.For<IUserAuthorizationService>();
    private readonly IAttachmentValidationService _attachmentValidator = Substitute.For<IAttachmentValidationService>();
    private readonly IGenreValidationService _genreValidator = Substitute.For<IGenreValidationService>();

    public UpdateMangaCommandHandlerTests()
    {
        _authService.HasModeratorPermissionAsync(Arg.Any<CancellationToken>()).Returns(true);
        _attachmentValidator.ValidateExistsAsync(Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((Result?)null));
        _genreValidator.ValidateAllExistAsync(Arg.Any<ICollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((Result?)null));
    }

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesTitle()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Old Title", AuthorId = author.Id };
        db.MangaSeries.Add(manga);
        await db.SaveChangesAsync();

        var handler = new UpdateMangaCommandHandler(db, _authService, _attachmentValidator, _genreValidator);
        var command = new UpdateMangaCommand(
            manga.Id, "New Title", null, null, null, null, null, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("New Title", db.MangaSeries.First().Title);
    }

    [Fact]
    public async Task Handle_MangaNotFound_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new UpdateMangaCommandHandler(db, _authService, _attachmentValidator, _genreValidator);
        var command = new UpdateMangaCommand(
            Guid.NewGuid(), "Title", null, null, null, null, null, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Manga not found", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_NonOwnerNonModerator_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Title", AuthorId = author.Id, CreatedBy = "user-1" };
        db.MangaSeries.Add(manga);
        await db.SaveChangesAsync();

        _authService.HasModeratorPermissionAsync(Arg.Any<CancellationToken>()).Returns(false);
        _authService.IsOwner("user-1").Returns(false);

        var handler = new UpdateMangaCommandHandler(db, _authService, _attachmentValidator, _genreValidator);
        var command = new UpdateMangaCommand(
            manga.Id, "New Title", null, null, null, null, null, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("your own manga", result.Errors[0]);
    }

    [Fact]
    public async Task Handle_OwnerCanUpdate()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Title", AuthorId = author.Id, CreatedBy = "user-1" };
        db.MangaSeries.Add(manga);
        await db.SaveChangesAsync();

        _authService.HasModeratorPermissionAsync(Arg.Any<CancellationToken>()).Returns(false);
        _authService.IsOwner("user-1").Returns(true);

        var handler = new UpdateMangaCommandHandler(db, _authService, _attachmentValidator, _genreValidator);
        var command = new UpdateMangaCommand(
            manga.Id, "Updated", null, null, null, null, null, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("Updated", db.MangaSeries.First().Title);
    }

    [Fact]
    public async Task Handle_ReplacesGenres()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        var oldGenre = new Genre { Name = "Action", Slug = "action" };
        var newGenre = new Genre { Name = "Comedy", Slug = "comedy" };
        db.Persons.Add(author);
        db.Genres.AddRange(oldGenre, newGenre);
        var manga = new MangaSeries { Title = "Title", AuthorId = author.Id };
        db.MangaSeries.Add(manga);
        db.MangaGenres.Add(new MangaGenre { MangaSeriesId = manga.Id, GenreId = oldGenre.Id });
        await db.SaveChangesAsync();

        var handler = new UpdateMangaCommandHandler(db, _authService, _attachmentValidator, _genreValidator);
        var command = new UpdateMangaCommand(
            manga.Id, null, null, null, [newGenre.Id], null, null, null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Succeeded);
        var genres = db.MangaGenres.Where(mg => mg.MangaSeriesId == manga.Id).ToList();
        Assert.Single(genres);
        Assert.Equal(newGenre.Id, genres[0].GenreId);
    }

    [Fact]
    public async Task Handle_InvalidCoverId_ReturnsFailure()
    {
        using var db = TestDbContextFactory.Create();
        var author = new Person { Name = "Author" };
        db.Persons.Add(author);
        var manga = new MangaSeries { Title = "Title", AuthorId = author.Id };
        db.MangaSeries.Add(manga);
        await db.SaveChangesAsync();

        var invalidAttachmentValidator = Substitute.For<IAttachmentValidationService>();
        invalidAttachmentValidator.ValidateExistsAsync(Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult((Result?)Result.Failure("Cover attachment not found")));

        var handler = new UpdateMangaCommandHandler(db, _authService, invalidAttachmentValidator, _genreValidator);
        var command = new UpdateMangaCommand(
            manga.Id, null, null, null, null, null, null, null, Guid.NewGuid(), null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Contains("Cover attachment not found", result.Errors[0]);
    }
}
