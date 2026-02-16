using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Chapters.Commands.DeleteChapter;

public class DeleteChapterCommandHandler(IAppDbContext db)
    : IRequestHandler<DeleteChapterCommand, Result>
{
    public async Task<Result> Handle(DeleteChapterCommand request, CancellationToken ct)
    {
        var chapter = await db.Chapters
            .Include(c => c.MangaSeries)
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct);

        if (chapter is null)
            return Result.Failure("Chapter not found.");

        // Soft delete
        chapter.IsDeleted = true;
        chapter.DeletedAt = DateTimeOffset.UtcNow;

        // Decrement denormalized count
        chapter.MangaSeries.TotalChapters = Math.Max(0, chapter.MangaSeries.TotalChapters - 1);

        // Recalculate latest chapter number (excluding the one being deleted)
        var latestChapterNumber = await db.Chapters
            .Where(c => c.MangaSeriesId == chapter.MangaSeriesId && c.Id != chapter.Id)
            .OrderByDescending(c => c.ChapterNumber)
            .Select(c => (int?)Math.Ceiling(c.ChapterNumber))
            .FirstOrDefaultAsync(ct);
        chapter.MangaSeries.LatestChapterNumber = latestChapterNumber ?? 0;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
