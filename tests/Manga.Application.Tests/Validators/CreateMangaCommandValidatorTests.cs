using FluentValidation.TestHelper;
using Manga.Application.Manga.Commands.CreateManga;
using Manga.Domain.Enums;

namespace Manga.Application.Tests.Validators;

public class CreateMangaCommandValidatorTests
{
    private readonly CreateMangaCommandValidator _validator = new();

    private static CreateMangaCommand ValidCommand() => new(
        "Test Manga", "Synopsis", Guid.NewGuid(), null,
        [Guid.NewGuid()], SeriesStatus.Ongoing, 2024, null, null);

    [Fact]
    public void Valid_Command_Passes()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Title_Empty_Fails(string? title)
    {
        var command = ValidCommand() with { Title = title! };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Title_TooLong_Fails()
    {
        var command = ValidCommand() with { Title = new string('a', 301) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 300 characters.");
    }

    [Fact]
    public void Synopsis_TooLong_Fails()
    {
        var command = ValidCommand() with { Synopsis = new string('a', 5001) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Synopsis)
            .WithErrorMessage("Synopsis must not exceed 5000 characters.");
    }

    [Fact]
    public void AuthorId_Empty_Fails()
    {
        var command = ValidCommand() with { AuthorId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AuthorId);
    }

    [Fact]
    public void GenreIds_Empty_Fails()
    {
        var command = ValidCommand() with { GenreIds = [] };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.GenreIds);
    }

    [Fact]
    public void GenreIds_TooMany_Fails()
    {
        var ids = Enumerable.Range(0, 11).Select(_ => Guid.NewGuid()).ToList();
        var command = ValidCommand() with { GenreIds = ids };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.GenreIds)
            .WithErrorMessage("Cannot assign more than 10 genres.");
    }

    [Fact]
    public void PublishedYear_OutOfRange_Fails()
    {
        var command = ValidCommand() with { PublishedYear = 1800 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PublishedYear);
    }

    [Fact]
    public void PublishedYear_Null_Passes()
    {
        var command = ValidCommand() with { PublishedYear = null };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.PublishedYear);
    }
}
