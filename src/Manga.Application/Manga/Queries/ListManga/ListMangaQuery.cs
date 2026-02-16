using Manga.Application.Common.Models;
using Manga.Application.Manga.DTOs;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Manga.Queries.ListManga;

/// <summary>
/// Lists manga with pagination, filtering, and sorting. Public endpoint.
/// </summary>
public record ListMangaQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? GenreId = null,
    SeriesStatus? Status = null,
    MangaSortBy SortBy = MangaSortBy.Latest,
    string? Search = null) : IRequest<Result<PagedResponse<MangaDto>>>;

public enum MangaSortBy
{
    Latest,
    Rating,
    Views,
    Title
}
