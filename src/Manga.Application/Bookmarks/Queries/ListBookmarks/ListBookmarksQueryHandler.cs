using Manga.Application.Bookmarks.DTOs;
using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Bookmarks.Queries.ListBookmarks;

public class ListBookmarksQueryHandler(
    IAppDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<ListBookmarksQuery, Result<PagedResponse<BookmarkDto>>>
{
    public async Task<Result<PagedResponse<BookmarkDto>>> Handle(
        ListBookmarksQuery request, CancellationToken ct)
    {
        var userId = Guid.Parse(currentUser.UserId!);

        var query = db.Bookmarks
            .Where(b => b.UserId == userId)
            .Include(b => b.MangaSeries)
                .ThenInclude(m => m.Cover)
            .OrderByDescending(b => b.CreatedAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BookmarkDto(
                b.Id,
                b.MangaSeriesId,
                b.MangaSeries.Title,
                b.MangaSeries.Cover != null ? b.MangaSeries.Cover.Url : null,
                b.CreatedAt))
            .ToListAsync(ct);

        var hasNext = request.Page * request.PageSize < totalCount;
        return Result<PagedResponse<BookmarkDto>>.Success(
            new(items, request.Page, request.PageSize, totalCount, hasNext));
    }
}
