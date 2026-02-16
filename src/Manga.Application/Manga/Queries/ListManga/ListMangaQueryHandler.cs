using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Application.Manga.DTOs;
using Manga.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Manga.Queries.ListManga;

public class ListMangaQueryHandler(IAppDbContext db)
    : IRequestHandler<ListMangaQuery, Result<PagedResponse<MangaDto>>>
{
    private const int MaxPageSize = 100;

    public async Task<Result<PagedResponse<MangaDto>>> Handle(ListMangaQuery request, CancellationToken ct)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var page = Math.Max(1, request.Page);

        // No Include() needed — Select() projection generates JOINs automatically
        IQueryable<MangaSeries> query = db.MangaSeries;

        // Filter by genre
        if (request.GenreId.HasValue)
        {
            query = query.Where(m =>
                m.MangaGenres.Any(mg => mg.GenreId == request.GenreId.Value));
        }

        // Filter by status
        if (request.Status.HasValue)
        {
            query = query.Where(m => m.Status == request.Status.Value);
        }

        // Sort
        query = request.SortBy switch
        {
            MangaSortBy.Rating => query.OrderByDescending(m => m.Rating),
            MangaSortBy.Views => query.OrderByDescending(m => m.Views),
            MangaSortBy.Title => query.OrderBy(m => m.Title),
            _ => query.OrderByDescending(m => m.CreatedAt), // Latest
        };

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MangaDto(
                m.Id,
                m.Title,
                m.Cover != null ? m.Cover.Url : null,
                m.Author.Name,
                m.Status,
                m.Badge,
                m.Rating,
                m.Views,
                m.TotalChapters,
                m.PublishedYear))
            .AsNoTracking()
            .ToListAsync(ct);

        var response = new PagedResponse<MangaDto>(
            items, page, pageSize, totalCount,
            page * pageSize < totalCount);

        return Result<PagedResponse<MangaDto>>.Success(response);
    }
}
