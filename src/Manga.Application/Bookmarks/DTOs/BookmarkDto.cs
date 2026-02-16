namespace Manga.Application.Bookmarks.DTOs;

/// <summary>
/// Bookmark list-view DTO with manga info for grids.
/// </summary>
public record BookmarkDto(
    Guid Id,
    Guid MangaSeriesId,
    string MangaTitle,
    string? CoverUrl,
    DateTimeOffset CreatedAt);

/// <summary>
/// Response after toggling a bookmark.
/// </summary>
public record ToggleBookmarkResponse(bool IsBookmarked, Guid? BookmarkId);
