using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Application.Manga.DTOs;
using Manga.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Manga.Queries.SearchManga;

public class SearchMangaQueryHandler(IAppDbContext db)
    : IRequestHandler<SearchMangaQuery, Result<PagedResponse<MangaDto>>>
{
    private const int MaxPageSize = 100;
    private const int MinSearchTermLength = 2;

    public async Task<Result<PagedResponse<MangaDto>>> Handle(
        SearchMangaQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm) || request.SearchTerm.Length < MinSearchTermLength)
            return Result<PagedResponse<MangaDto>>.Failure("Search term must be at least 2 characters.");

        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var page = Math.Max(1, request.Page);
        var term = request.SearchTerm.Trim().ToLower();

        // Case-insensitive search via ToLower (EF translates to LOWER() in SQL)
        IQueryable<MangaSeries> query = db.MangaSeries
            .Where(m =>
                m.Title.ToLower().Contains(term) ||
                m.Author.Name.ToLower().Contains(term) ||
                m.AlternativeTitles.Any(at => at.Title.ToLower().Contains(term)));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(m => m.Title.ToLower() == term ? 0 : 1)
            .ThenByDescending(m => m.Views)
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
