using FluentValidation;

namespace Manga.Application.Views.Commands.TrackView;

public class TrackViewCommandValidator : AbstractValidator<TrackViewCommand>
{
    public TrackViewCommandValidator()
    {
        RuleFor(x => x.TargetId)
            .NotEmpty().WithMessage("TargetId is required.");

        RuleFor(x => x.TargetType)
            .IsInEnum().WithMessage("TargetType must be a valid value (0=Series, 1=Chapter).");

        RuleFor(x => x.ViewerIdentifier)
            .NotEmpty().WithMessage("ViewerIdentifier is required.");
    }
}
