using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.ReadingHistories.Commands.UpsertReadingHistory;

public class UpsertReadingHistoryCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<UpsertReadingHistoryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        UpsertReadingHistoryCommand request, CancellationToken ct)
    {
        var userId = Guid.Parse(currentUser.UserId!);

        // Validate manga and chapter exist
        var mangaExists = await db.MangaSeries.AnyAsync(m => m.Id == request.MangaSeriesId, ct);
        if (!mangaExists)
            return Result<Guid>.Failure("Manga series not found.");

        var chapterExists = await db.Chapters.AnyAsync(c => c.Id == request.ChapterId, ct);
        if (!chapterExists)
            return Result<Guid>.Failure("Chapter not found.");

        // Find existing record (upsert)
        var existing = await db.ReadingHistories
            .FirstOrDefaultAsync(rh => rh.UserId == userId && rh.MangaSeriesId == request.MangaSeriesId, ct);

        if (existing is not null)
        {
            existing.ChapterId = request.ChapterId;
            existing.LastPageNumber = request.LastPageNumber;
            existing.LastReadAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return Result<Guid>.Success(existing.Id);
        }

        var history = new Domain.Entities.ReadingHistory
        {
            UserId = userId,
            MangaSeriesId = request.MangaSeriesId,
            ChapterId = request.ChapterId,
            LastPageNumber = request.LastPageNumber,
            LastReadAt = DateTimeOffset.UtcNow,
        };
        db.ReadingHistories.Add(history);
        await db.SaveChangesAsync(ct);

        return Result<Guid>.Success(history.Id);
    }
}
