using FluentValidation;

namespace Manga.Application.Chapters.Commands.UpdateChapter;

public class UpdateChapterCommandValidator : AbstractValidator<UpdateChapterCommand>
{
    public UpdateChapterCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Chapter ID is required.");

        RuleFor(x => x.Title)
            .MaximumLength(300).WithMessage("Title must not exceed 300 characters.")
            .When(x => x.Title is not null);

        RuleFor(x => x.PageImageIds)
            .Must(ids => ids!.Count > 0).WithMessage("Page list cannot be empty.")
            .Must(ids => ids!.Count <= 500).WithMessage("Cannot exceed 500 pages per chapter.")
            .Must(ids => ids!.Distinct().Count() == ids.Count).WithMessage("Duplicate page image IDs are not allowed.")
            .When(x => x.PageImageIds is not null);
    }
}
