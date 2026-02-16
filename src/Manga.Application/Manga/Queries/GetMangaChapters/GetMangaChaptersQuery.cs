using Manga.Application.Common.Models;
using Manga.Application.Manga.DTOs;
using MediatR;

namespace Manga.Application.Manga.Queries.GetMangaChapters;

/// <summary>
/// Lists chapters for a manga, ordered by chapter number ascending. Public endpoint.
/// </summary>
public record GetMangaChaptersQuery(
    Guid MangaId,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<PagedResponse<ChapterDto>>>;
