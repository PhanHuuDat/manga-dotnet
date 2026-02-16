using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using MediatR;

namespace Manga.Application.ReadingHistories.Commands.UpsertReadingHistory;

/// <summary>
/// Create or update reading progress for a manga series.
/// </summary>
[Authorize]
public record UpsertReadingHistoryCommand(
    Guid MangaSeriesId,
    Guid ChapterId,
    int LastPageNumber) : IRequest<Result<Guid>>;
