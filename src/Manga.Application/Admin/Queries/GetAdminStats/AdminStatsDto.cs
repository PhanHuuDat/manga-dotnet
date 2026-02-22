namespace Manga.Application.Admin.Queries.GetAdminStats;

/// <summary>
/// Platform-wide statistics for the admin dashboard.
/// </summary>
public record AdminStatsDto(
    int TotalManga,
    int TotalChapters,
    int TotalUsers,
    int TotalComments,
    int NewUsersLast7Days);
