using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Application.ReadingHistories.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.ReadingHistories.Queries.ListReadingHistory;

public class ListReadingHistoryQueryHandler(
    IAppDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<ListReadingHistoryQuery, Result<PagedResponse<ReadingHistoryDto>>>
{
    public async Task<Result<PagedResponse<ReadingHistoryDto>>> Handle(
        ListReadingHistoryQuery request, CancellationToken ct)
    {
        var userId = Guid.Parse(currentUser.UserId!);

        var query = db.ReadingHistories
            .Where(rh => rh.UserId == userId)
            .OrderByDescending(rh => rh.LastReadAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(rh => new ReadingHistoryDto(
                rh.Id,
                rh.MangaSeriesId,
                rh.MangaSeries.Title,
                rh.MangaSeries.Cover != null ? rh.MangaSeries.Cover.Url : null,
                rh.ChapterId,
                rh.Chapter.Title,
                rh.Chapter.ChapterNumber,
                rh.LastPageNumber,
                rh.LastReadAt))
            .ToListAsync(ct);

        var hasNext = request.Page * request.PageSize < totalCount;
        return Result<PagedResponse<ReadingHistoryDto>>.Success(
            new(items, request.Page, request.PageSize, totalCount, hasNext));
    }
}
