using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Application.ReadingHistories.DTOs;
using MediatR;

namespace Manga.Application.ReadingHistories.Queries.ListReadingHistory;

/// <summary>
/// List current user's reading history, sorted by most recently read.
/// </summary>
[Authorize]
public record ListReadingHistoryQuery(int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResponse<ReadingHistoryDto>>>;
