using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Bookmarks.Queries.CheckBookmark;

public class CheckBookmarkQueryHandler(
    IAppDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<CheckBookmarkQuery, Result<bool>>
{
    public async Task<Result<bool>> Handle(CheckBookmarkQuery request, CancellationToken ct)
    {
        var userId = Guid.Parse(currentUser.UserId!);

        var isBookmarked = await db.Bookmarks
            .AnyAsync(b => b.UserId == userId && b.MangaSeriesId == request.MangaSeriesId, ct);

        return Result<bool>.Success(isBookmarked);
    }
}
