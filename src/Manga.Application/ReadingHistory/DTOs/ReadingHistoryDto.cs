namespace Manga.Application.ReadingHistories.DTOs;

/// <summary>
/// Reading history list-view DTO with manga and chapter info.
/// </summary>
public record ReadingHistoryDto(
    Guid Id,
    Guid MangaSeriesId,
    string MangaTitle,
    string? CoverUrl,
    Guid ChapterId,
    string? ChapterTitle,
    decimal ChapterNumber,
    int LastPageNumber,
    DateTimeOffset LastReadAt);

/// <summary>
/// Resume point for continuing reading a specific manga.
/// </summary>
public record ResumePointDto(
    Guid ChapterId,
    string? ChapterTitle,
    decimal ChapterNumber,
    int LastPageNumber);
