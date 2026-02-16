using Manga.Domain.Enums;

namespace Manga.Application.Manga.DTOs;

/// <summary>
/// Manga list-view DTO with essential fields for cards and grids.
/// </summary>
public record MangaDto(
    Guid Id,
    string Title,
    string? CoverUrl,
    string AuthorName,
    SeriesStatus Status,
    MangaBadge? Badge,
    decimal Rating,
    long Views,
    int TotalChapters,
    int? PublishedYear);
