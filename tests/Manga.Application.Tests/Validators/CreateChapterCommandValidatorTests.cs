using FluentValidation.TestHelper;
using Manga.Application.Chapters.Commands.CreateChapter;

namespace Manga.Application.Tests.Validators;

public class CreateChapterCommandValidatorTests
{
    private readonly CreateChapterCommandValidator _validator = new();

    private static CreateChapterCommand ValidCommand() => new(
        Guid.NewGuid(), 1, "Chapter 1", DateTimeOffset.UtcNow, [Guid.NewGuid()]);

    [Fact]
    public void Valid_Command_Passes()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void MangaSeriesId_Empty_Fails()
    {
        var command = ValidCommand() with { MangaSeriesId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.MangaSeriesId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChapterNumber_NotPositive_Fails(decimal number)
    {
        var command = ValidCommand() with { ChapterNumber = number };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ChapterNumber);
    }

    [Fact]
    public void Title_TooLong_Fails()
    {
        var command = ValidCommand() with { Title = new string('a', 301) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Title_Null_Passes()
    {
        var command = ValidCommand() with { Title = null };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void PageImageIds_Empty_Fails()
    {
        var command = ValidCommand() with { PageImageIds = [] };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PageImageIds);
    }

    [Fact]
    public void PageImageIds_TooMany_Fails()
    {
        var ids = Enumerable.Range(0, 501).Select(_ => Guid.NewGuid()).ToList();
        var command = ValidCommand() with { PageImageIds = ids };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PageImageIds)
            .WithErrorMessage("Cannot exceed 500 pages per chapter.");
    }
}
