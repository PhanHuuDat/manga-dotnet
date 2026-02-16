using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Application.Manga.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Manga.Queries.GetMangaChapters;

public class GetMangaChaptersQueryHandler(IAppDbContext db)
    : IRequestHandler<GetMangaChaptersQuery, Result<PagedResponse<ChapterDto>>>
{
    private const int MaxPageSize = 100;

    public async Task<Result<PagedResponse<ChapterDto>>> Handle(
        GetMangaChaptersQuery request, CancellationToken ct)
    {
        // Verify manga exists
        var mangaExists = await db.MangaSeries.AnyAsync(m => m.Id == request.MangaId, ct);
        if (!mangaExists)
            return Result<PagedResponse<ChapterDto>>.Failure("Manga not found.");

        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var page = Math.Max(1, request.Page);

        var query = db.Chapters
            .Where(c => c.MangaSeriesId == request.MangaId)
            .OrderBy(c => c.ChapterNumber)
            .AsNoTracking();

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ChapterDto(
                c.Id,
                c.ChapterNumber,
                c.Title,
                c.Slug,
                c.Pages,
                c.Views,
                c.PublishedAt))
            .ToListAsync(ct);

        var response = new PagedResponse<ChapterDto>(
            items, page, pageSize, totalCount,
            page * pageSize < totalCount);

        return Result<PagedResponse<ChapterDto>>.Success(response);
    }
}
