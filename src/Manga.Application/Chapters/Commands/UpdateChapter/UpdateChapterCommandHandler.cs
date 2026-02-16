using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Application.Common.Services;
using Manga.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Chapters.Commands.UpdateChapter;

public class UpdateChapterCommandHandler(
    IAppDbContext db,
    IUserAuthorizationService authService)
    : IRequestHandler<UpdateChapterCommand, Result>
{
    public async Task<Result> Handle(UpdateChapterCommand request, CancellationToken ct)
    {
        var chapter = await db.Chapters
            .Include(c => c.MangaSeries)
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct);

        if (chapter is null)
            return Result.Failure("Chapter not found.");

        // Ownership check: Uploaders can only update chapters of own manga
        if (!await authService.HasModeratorPermissionAsync(ct))
        {
            if (!authService.IsOwner(chapter.MangaSeries.CreatedBy))
                return Result.Failure("You can only update chapters of your own manga.");
        }

        if (request.Title is not null) chapter.Title = request.Title;
        if (request.PublishedAt.HasValue) chapter.PublishedAt = request.PublishedAt.Value;

        // Replace pages if provided
        if (request.PageImageIds is not null)
        {
            // Validate all page image IDs exist
            var validImageCount = await db.Attachments
                .CountAsync(a => request.PageImageIds.Contains(a.Id), ct);
            if (validImageCount != request.PageImageIds.Count)
                return Result.Failure("One or more page images not found.");

            var oldPages = await db.ChapterPages
                .Where(p => p.ChapterId == chapter.Id)
                .ToListAsync(ct);
            db.ChapterPages.RemoveRange(oldPages);

            for (var i = 0; i < request.PageImageIds.Count; i++)
            {
                db.ChapterPages.Add(new ChapterPage
                {
                    ChapterId = chapter.Id,
                    PageNumber = i + 1,
                    ImageId = request.PageImageIds[i],
                });
            }

            chapter.Pages = request.PageImageIds.Count;
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
