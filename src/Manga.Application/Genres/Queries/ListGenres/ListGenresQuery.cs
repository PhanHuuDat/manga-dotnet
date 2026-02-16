using Manga.Application.Common.Models;
using Manga.Application.Genres.DTOs;
using MediatR;

namespace Manga.Application.Genres.Queries.ListGenres;

/// <summary>
/// Returns all genres with manga count. Public endpoint. Cached 24h.
/// </summary>
public record ListGenresQuery : IRequest<Result<IReadOnlyList<GenreWithCountDto>>>;
