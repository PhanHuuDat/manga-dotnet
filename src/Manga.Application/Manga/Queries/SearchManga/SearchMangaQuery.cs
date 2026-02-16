using Manga.Application.Common.Models;
using Manga.Application.Manga.DTOs;
using MediatR;

namespace Manga.Application.Manga.Queries.SearchManga;

/// <summary>
/// Searches manga by title, author name, or alternative titles. Public endpoint.
/// </summary>
public record SearchMangaQuery(
    string SearchTerm,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResponse<MangaDto>>>;
