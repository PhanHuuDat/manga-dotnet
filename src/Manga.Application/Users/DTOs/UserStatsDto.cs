namespace Manga.Application.Users.DTOs;

/// <summary>
/// User statistics including bookmark, history, and comment counts.
/// </summary>
public record UserStatsDto(int BookmarkCount, int HistoryCount, int CommentCount);
