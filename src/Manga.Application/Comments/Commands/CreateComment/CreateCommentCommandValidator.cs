using FluentValidation;

namespace Manga.Application.Comments.Commands.CreateComment;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);

        // At least one target must be set
        RuleFor(x => x)
            .Must(x => x.MangaSeriesId.HasValue || x.ChapterId.HasValue)
            .WithMessage("Either MangaSeriesId or ChapterId must be provided.");

        // PageNumber only valid with ChapterId
        RuleFor(x => x.PageNumber)
            .Null()
            .When(x => !x.ChapterId.HasValue)
            .WithMessage("PageNumber requires ChapterId.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .When(x => x.PageNumber.HasValue);
    }
}
