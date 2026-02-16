using FluentValidation;

namespace Manga.Application.Chapters.Commands.CreateChapter;

public class CreateChapterCommandValidator : AbstractValidator<CreateChapterCommand>
{
    public CreateChapterCommandValidator()
    {
        RuleFor(x => x.MangaSeriesId)
            .NotEmpty().WithMessage("Manga series ID is required.");

        RuleFor(x => x.ChapterNumber)
            .GreaterThan(0).WithMessage("Chapter number must be greater than 0.");

        RuleFor(x => x.Title)
            .MaximumLength(300).WithMessage("Title must not exceed 300 characters.")
            .When(x => x.Title is not null);

        RuleFor(x => x.PageImageIds)
            .NotEmpty().WithMessage("At least one page image is required.")
            .Must(ids => ids.Count <= 500).WithMessage("Cannot exceed 500 pages per chapter.")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Duplicate page image IDs are not allowed.");
    }
}
