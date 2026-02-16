using FluentValidation;

namespace Manga.Application.Manga.Commands.CreateManga;

public class CreateMangaCommandValidator : AbstractValidator<CreateMangaCommand>
{
    public CreateMangaCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(300).WithMessage("Title must not exceed 300 characters.");

        RuleFor(x => x.Synopsis)
            .MaximumLength(5000).WithMessage("Synopsis must not exceed 5000 characters.");

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("Author is required.");

        RuleFor(x => x.GenreIds)
            .NotEmpty().WithMessage("At least one genre is required.")
            .Must(ids => ids.Count <= 10).WithMessage("Cannot assign more than 10 genres.")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Duplicate genre IDs are not allowed.");

        RuleFor(x => x.PublishedYear)
            .InclusiveBetween(1900, 2100)
            .When(x => x.PublishedYear.HasValue)
            .WithMessage("Published year must be between 1900 and 2100.");
    }
}
