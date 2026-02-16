namespace Manga.Application.Manga.DTOs;

/// <summary>
/// Lightweight person (author/artist) representation for API responses.
/// </summary>
public record PersonDto(
    Guid Id,
    string Name,
    string? Biography,
    string? PhotoUrl);
