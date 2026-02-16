namespace Manga.Application.Manga.DTOs;

/// <summary>
/// Genre tag representation for API responses.
/// </summary>
public record GenreDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description);
