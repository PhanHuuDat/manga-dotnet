using FluentValidation;

namespace Manga.Application.Manga.Commands.UpdateManga;

public class UpdateMangaCommandValidator : AbstractValidator<UpdateMangaCommand>
{
    public UpdateMangaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Manga ID is required.");

        RuleFor(x => x.Title)
            .MaximumLength(300).WithMessage("Title must not exceed 300 characters.")
            .When(x => x.Title is not null);

        RuleFor(x => x.Synopsis)
            .MaximumLength(5000).WithMessage("Synopsis must not exceed 5000 characters.")
            .When(x => x.Synopsis is not null);

        RuleFor(x => x.GenreIds)
            .Must(ids => ids!.Count > 0).WithMessage("Genre list cannot be empty.")
            .Must(ids => ids!.Count <= 10).WithMessage("Cannot assign more than 10 genres.")
            .When(x => x.GenreIds is not null);

        RuleFor(x => x.PublishedYear)
            .InclusiveBetween(1900, 2100)
            .When(x => x.PublishedYear.HasValue)
            .WithMessage("Published year must be between 1900 and 2100.");
    }
}
