namespace Manga.Application.Manga.DTOs;

/// <summary>
/// Chapter list-view DTO for chapter listings on manga detail page.
/// </summary>
public record ChapterDto(
    Guid Id,
    decimal ChapterNumber,
    string? Title,
    string Slug,
    int Pages,
    long Views,
    DateTimeOffset PublishedAt);
