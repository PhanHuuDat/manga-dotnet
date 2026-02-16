using Manga.Domain.Enums;

namespace Manga.Application.Manga.DTOs;

/// <summary>
/// Full manga detail DTO with author, artist, genres, and stats.
/// </summary>
public record MangaDetailDto(
    Guid Id,
    string Title,
    string? Synopsis,
    string? CoverUrl,
    string? BannerUrl,
    PersonDto Author,
    PersonDto? Artist,
    IReadOnlyList<GenreDto> Genres,
    IReadOnlyList<string> AlternativeTitles,
    SeriesStatus Status,
    MangaBadge? Badge,
    int? PublishedYear,
    decimal Rating,
    int RatingCount,
    long Views,
    int TotalChapters,
    int LatestChapterNumber,
    DateTimeOffset CreatedAt);
