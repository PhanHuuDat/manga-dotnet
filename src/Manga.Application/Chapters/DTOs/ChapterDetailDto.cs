namespace Manga.Application.Chapters.DTOs;

/// <summary>
/// Full chapter detail with pages for the reader view.
/// </summary>
public record ChapterDetailDto(
    Guid Id,
    Guid MangaSeriesId,
    string MangaTitle,
    decimal ChapterNumber,
    string? Title,
    string Slug,
    DateTimeOffset PublishedAt,
    IReadOnlyList<ChapterPageDto> Pages,
    long Views,
    DateTimeOffset CreatedAt);

public record ChapterPageDto(
    Guid Id,
    int PageNumber,
    string ImageUrl);
