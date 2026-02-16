namespace Manga.Application.Genres.DTOs;

/// <summary>
/// Genre with manga count for filtering UI.
/// </summary>
public record GenreWithCountDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    int MangaCount);
