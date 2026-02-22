using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Admin.Queries.GetAdminStats;

/// <summary>
/// Counts platform-wide entities for the admin stats dashboard.
/// </summary>
public class GetAdminStatsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetAdminStatsQuery, Result<AdminStatsDto>>
{
    public async Task<Result<AdminStatsDto>> Handle(GetAdminStatsQuery request, CancellationToken ct)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-7);

        var totalManga = await db.MangaSeries.CountAsync(ct);
        var totalChapters = await db.Chapters.CountAsync(ct);
        var totalUsers = await db.Users.CountAsync(ct);
        var totalComments = await db.Comments.CountAsync(ct);
        var newUsersLast7Days = await db.Users
            .CountAsync(u => u.CreatedAt >= cutoff, ct);

        var dto = new AdminStatsDto(
            totalManga,
            totalChapters,
            totalUsers,
            totalComments,
            newUsersLast7Days);

        return Result<AdminStatsDto>.Success(dto);
    }
}
