using Manga.Application.Chapters.DTOs;
using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Chapters.Queries.GetChapter;

public class GetChapterQueryHandler(IAppDbContext db)
    : IRequestHandler<GetChapterQuery, Result<ChapterDetailDto>>
{
    public async Task<Result<ChapterDetailDto>> Handle(GetChapterQuery request, CancellationToken ct)
    {
        var chapter = await db.Chapters
            .Include(c => c.MangaSeries)
            .Include(c => c.ChapterPages.OrderBy(p => p.PageNumber))
                .ThenInclude(p => p.Image)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct);

        if (chapter is null)
            return Result<ChapterDetailDto>.Failure("Chapter not found.");

        var dto = new ChapterDetailDto(
            chapter.Id,
            chapter.MangaSeriesId,
            chapter.MangaSeries.Title,
            chapter.ChapterNumber,
            chapter.Title,
            chapter.Slug,
            chapter.PublishedAt,
            chapter.ChapterPages.Select(p => new ChapterPageDto(
                p.Id, p.PageNumber, p.Image.Url)).ToList(),
            chapter.Views,
            chapter.CreatedAt);

        return Result<ChapterDetailDto>.Success(dto);
    }
}
