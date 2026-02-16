using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.ReadingHistories.Commands.ClearReadingHistory;

public class ClearReadingHistoryCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<ClearReadingHistoryCommand, Result>
{
    public async Task<Result> Handle(ClearReadingHistoryCommand request, CancellationToken ct)
    {
        var userId = Guid.Parse(currentUser.UserId!);

        var existing = await db.ReadingHistories
            .FirstOrDefaultAsync(rh => rh.UserId == userId && rh.MangaSeriesId == request.MangaSeriesId, ct);

        if (existing is not null)
        {
            db.ReadingHistories.Remove(existing);
            await db.SaveChangesAsync(ct);
        }

        // Idempotent — success even if not found
        return Result.Success();
    }
}
