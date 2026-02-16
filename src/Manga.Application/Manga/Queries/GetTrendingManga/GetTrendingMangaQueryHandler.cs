using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Application.Manga.DTOs;
using Manga.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Manga.Application.Manga.Queries.GetTrendingManga;

public class GetTrendingMangaQueryHandler(IAppDbContext db, IMemoryCache cache)
    : IRequestHandler<GetTrendingMangaQuery, Result<PagedResponse<MangaDto>>>
{
    private const int MaxPageSize = 100;

    public async Task<Result<PagedResponse<MangaDto>>> Handle(
        GetTrendingMangaQuery request, CancellationToken ct)
    {
        var days = request.Days is 7 or 30 ? request.Days : 7;
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var page = Math.Max(1, request.Page);

        var cacheKey = $"trending_{days}_{page}_{pageSize}";
        if (cache.TryGetValue(cacheKey, out PagedResponse<MangaDto>? cached))
            return Result<PagedResponse<MangaDto>>.Success(cached!);

        var cutoffDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));

        // Aggregate view stats and join with manga
        var query = from m in db.MangaSeries.Include(m => m.Author).Include(m => m.Cover)
                    join vs in db.ViewStats
                        on new { TargetId = m.Id, TargetType = ViewTargetType.Series }
                        equals new { vs.TargetId, vs.TargetType }
                        into viewStats
                    from vs in viewStats.DefaultIfEmpty()
                    where vs == null || vs.ViewDate >= cutoffDate
                    group new { m, vs } by new
                    {
                        m.Id, m.Title, m.Status, m.Badge, m.Rating, m.Views,
                        m.TotalChapters, m.PublishedYear,
                        AuthorName = m.Author.Name,
                        CoverUrl = m.Cover != null ? m.Cover.Url : null,
                    } into g
                    orderby g.Sum(x => x.vs != null ? x.vs.ViewCount : 0) descending
                    select new MangaDto(
                        g.Key.Id,
                        g.Key.Title,
                        g.Key.CoverUrl,
                        g.Key.AuthorName,
                        g.Key.Status,
                        g.Key.Badge,
                        g.Key.Rating,
                        g.Key.Views,
                        g.Key.TotalChapters,
                        g.Key.PublishedYear);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var response = new PagedResponse<MangaDto>(
            items, page, pageSize, totalCount,
            page * pageSize < totalCount);

        cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

        return Result<PagedResponse<MangaDto>>.Success(response);
    }
}
