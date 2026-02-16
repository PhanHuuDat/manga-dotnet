using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Application.Users.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Users.Queries.GetUserStats;

public class GetUserStatsQueryHandler(
    IAppDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<GetUserStatsQuery, Result<UserStatsDto>>
{
    public async Task<Result<UserStatsDto>> Handle(
        GetUserStatsQuery request, CancellationToken ct)
    {
        if (!Guid.TryParse(currentUser.UserId, out var userId))
            return Result<UserStatsDto>.Failure("User not authenticated.");

        var bookmarkCount = await db.Bookmarks
            .Where(b => b.UserId == userId)
            .CountAsync(ct);

        var historyCount = await db.ReadingHistories
            .Where(h => h.UserId == userId)
            .CountAsync(ct);

        var commentCount = await db.Comments
            .Where(c => c.UserId == userId)
            .CountAsync(ct);

        var stats = new UserStatsDto(bookmarkCount, historyCount, commentCount);
        return Result<UserStatsDto>.Success(stats);
    }
}
