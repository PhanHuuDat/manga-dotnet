using FluentValidation;

namespace Manga.Application.ReadingHistories.Commands.UpsertReadingHistory;

public class UpsertReadingHistoryCommandValidator : AbstractValidator<UpsertReadingHistoryCommand>
{
    public UpsertReadingHistoryCommandValidator()
    {
        RuleFor(x => x.MangaSeriesId).NotEmpty();
        RuleFor(x => x.ChapterId).NotEmpty();
        RuleFor(x => x.LastPageNumber).GreaterThanOrEqualTo(1);
    }
}
