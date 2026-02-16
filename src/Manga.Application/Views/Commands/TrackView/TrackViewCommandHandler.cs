using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Views.Commands.TrackView;

public class TrackViewCommandHandler(
    IAppDbContext db,
    IViewTrackingService viewTracking)
    : IRequestHandler<TrackViewCommand, Result>
{
    public async Task<Result> Handle(TrackViewCommand request, CancellationToken ct)
    {
        var viewDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Track in Redis HyperLogLog and get unique count (-1 = Redis unavailable)
        var uniqueCount = await viewTracking.TrackAndGetUniqueCountAsync(
            request.TargetType, request.TargetId, viewDate,
            request.ViewerIdentifier, ct);

        // Upsert ViewStat row with retry for concurrent insert race condition
        try
        {
            await UpsertViewStatAsync(request, viewDate, uniqueCount, ct);
        }
        catch (DbUpdateException)
        {
            // Concurrent insert hit unique constraint — retry as update.
            // The failed Add left a tracked entity; re-query will find the row
            // inserted by the other request and update it instead.
            var existing = await db.ViewStats.FirstOrDefaultAsync(
                v => v.TargetType == request.TargetType
                     && v.TargetId == request.TargetId
                     && v.ViewDate == viewDate, ct);
            if (existing is not null)
            {
                existing.ViewCount++;
                if (uniqueCount >= 0)
                    existing.UniqueViewCount = uniqueCount;
                await db.SaveChangesAsync(ct);
            }
        }

        return Result.Success();
    }

    private async Task UpsertViewStatAsync(
        TrackViewCommand request, DateOnly viewDate, long uniqueCount, CancellationToken ct)
    {
        var existing = await db.ViewStats.FirstOrDefaultAsync(
            v => v.TargetType == request.TargetType
                 && v.TargetId == request.TargetId
                 && v.ViewDate == viewDate, ct);

        if (existing is null)
        {
            db.ViewStats.Add(new ViewStat
            {
                TargetType = request.TargetType,
                TargetId = request.TargetId,
                ViewDate = viewDate,
                ViewCount = 1,
                UniqueViewCount = Math.Max(uniqueCount, 0),
            });
        }
        else
        {
            existing.ViewCount++;
            // Only update UniqueViewCount if Redis returned a valid value
            if (uniqueCount >= 0)
                existing.UniqueViewCount = uniqueCount;
        }

        await db.SaveChangesAsync(ct);
    }
}
