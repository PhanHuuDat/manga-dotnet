using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Application.Common.Services;
using Manga.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Chapters.Commands.CreateChapter;

public class CreateChapterCommandHandler(
    IAppDbContext db,
    IUserAuthorizationService authService)
    : IRequestHandler<CreateChapterCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateChapterCommand request, CancellationToken ct)
    {
        var manga = await db.MangaSeries
            .FirstOrDefaultAsync(m => m.Id == request.MangaSeriesId, ct);

        if (manga is null)
            return Result<Guid>.Failure("Manga series not found.");

        // Ownership check: Uploaders can only add chapters to own manga
        if (!await authService.HasModeratorPermissionAsync(ct))
        {
            if (!authService.IsOwner(manga.CreatedBy))
                return Result<Guid>.Failure("You can only add chapters to your own manga.");
        }

        // Validate chapter number is unique for this manga
        var chapterExists = await db.Chapters
            .AnyAsync(c => c.MangaSeriesId == request.MangaSeriesId
                        && c.ChapterNumber == request.ChapterNumber, ct);
        if (chapterExists)
            return Result<Guid>.Failure($"Chapter {request.ChapterNumber} already exists for this manga.");

        // Validate all page image IDs exist as attachments
        var validImageCount = await db.Attachments
            .CountAsync(a => request.PageImageIds.Contains(a.Id), ct);
        if (validImageCount != request.PageImageIds.Count)
            return Result<Guid>.Failure("One or more page images not found.");

        var slug = string.Create(System.Globalization.CultureInfo.InvariantCulture,
            $"chapter-{request.ChapterNumber:F1}").Replace(".0", "");

        var chapter = new Chapter
        {
            MangaSeriesId = request.MangaSeriesId,
            ChapterNumber = request.ChapterNumber,
            Title = request.Title,
            Slug = slug,
            PublishedAt = request.PublishedAt,
            Pages = request.PageImageIds.Count,
        };

        db.Chapters.Add(chapter);

        for (var i = 0; i < request.PageImageIds.Count; i++)
        {
            db.ChapterPages.Add(new ChapterPage
            {
                ChapterId = chapter.Id,
                PageNumber = i + 1,
                ImageId = request.PageImageIds[i],
            });
        }

        // Update denormalized counts
        manga.TotalChapters += 1;
        var chapterInt = (int)Math.Ceiling(request.ChapterNumber);
        if (chapterInt > manga.LatestChapterNumber)
            manga.LatestChapterNumber = chapterInt;

        await db.SaveChangesAsync(ct);
        return Result<Guid>.Success(chapter.Id);
    }
}
