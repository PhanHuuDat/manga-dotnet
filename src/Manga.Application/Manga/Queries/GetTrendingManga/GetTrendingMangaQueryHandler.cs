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
    private const int TrendingListCap = 200;

    public async Task<Result<PagedResponse<MangaDto>>> Handle(
        GetTrendingMangaQuery request, CancellationToken ct)
    {
        var days = request.Days is 7 or 30 ? request.Days : 7;
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var page = Math.Max(1, request.Page);

        // Cache full trending list per time window; paginate in-memory to avoid per-page cache fragmentation
        var cacheKey = $"trending_{days}";
        if (!cache.TryGetValue(cacheKey, out List<MangaDto>? allItems) || allItems is null)
        {
            var cutoffDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));

            // Aggregate view stats and join with manga, cap at TrendingListCap rows
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
                        orderby g.Sum(x => x.vs != null ? x.vs.UniqueViewCount : 0) descending
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

            allItems = await query.Take(TrendingListCap).ToListAsync(ct);

            cache.Set(cacheKey, allItems, TimeSpan.FromMinutes(5));
        }

        // Paginate the cached list in-memory
        var totalCount = allItems.Count;
        var items = allItems
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var response = new PagedResponse<MangaDto>(
            items, page, pageSize, totalCount,
            page * pageSize < totalCount);

        return Result<PagedResponse<MangaDto>>.Success(response);
    }
}
