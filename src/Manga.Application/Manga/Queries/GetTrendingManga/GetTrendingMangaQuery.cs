using Manga.Application.Common.Models;
using Manga.Application.Manga.DTOs;
using MediatR;

namespace Manga.Application.Manga.Queries.GetTrendingManga;

/// <summary>
/// Returns trending manga based on ViewStat aggregation over N days. Public endpoint.
/// </summary>
public record GetTrendingMangaQuery(
    int Days = 7,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResponse<MangaDto>>>;
