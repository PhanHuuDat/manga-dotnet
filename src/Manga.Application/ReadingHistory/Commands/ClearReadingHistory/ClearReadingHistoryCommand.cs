using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using MediatR;

namespace Manga.Application.ReadingHistories.Commands.ClearReadingHistory;

/// <summary>
/// Clear reading history for a specific manga (hard delete).
/// </summary>
[Authorize]
public record ClearReadingHistoryCommand(Guid MangaSeriesId) : IRequest<Result>;
