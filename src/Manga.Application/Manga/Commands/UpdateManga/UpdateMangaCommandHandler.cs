using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Application.Common.Services;
using Manga.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Manga.Commands.UpdateManga;

public class UpdateMangaCommandHandler(
    IAppDbContext db,
    IUserAuthorizationService authService)
    : IRequestHandler<UpdateMangaCommand, Result>
{
    public async Task<Result> Handle(UpdateMangaCommand request, CancellationToken ct)
    {
        var manga = await db.MangaSeries
            .FirstOrDefaultAsync(m => m.Id == request.Id, ct);

        if (manga is null)
            return Result.Failure("Manga not found.");

        // Ownership check: Uploaders can only update own manga
        if (!await authService.HasModeratorPermissionAsync(ct))
        {
            if (!authService.IsOwner(manga.CreatedBy))
                return Result.Failure("You can only update your own manga.");
        }

        // Update provided fields
        if (request.Title is not null) manga.Title = request.Title;
        if (request.Synopsis is not null) manga.Synopsis = request.Synopsis;
        if (request.ArtistId.HasValue) manga.ArtistId = request.ArtistId;
        if (request.Status.HasValue) manga.Status = request.Status.Value;
        if (request.Badge.HasValue) manga.Badge = request.Badge;
        if (request.PublishedYear.HasValue) manga.PublishedYear = request.PublishedYear;

        // Validate and set cover/banner attachments
        if (request.CoverId.HasValue)
        {
            if (!await db.Attachments.AnyAsync(a => a.Id == request.CoverId.Value, ct))
                return Result.Failure("Cover attachment not found.");
            manga.CoverId = request.CoverId;
        }
        if (request.BannerId.HasValue)
        {
            if (!await db.Attachments.AnyAsync(a => a.Id == request.BannerId.Value, ct))
                return Result.Failure("Banner attachment not found.");
            manga.BannerId = request.BannerId;
        }

        // Replace genres if provided
        if (request.GenreIds is not null)
        {
            var existingGenreCount = await db.Genres
                .CountAsync(g => request.GenreIds.Contains(g.Id), ct);
            if (existingGenreCount != request.GenreIds.Count)
                return Result.Failure("One or more genres not found.");

            var oldGenres = await db.MangaGenres
                .Where(mg => mg.MangaSeriesId == manga.Id)
                .ToListAsync(ct);
            db.MangaGenres.RemoveRange(oldGenres);

            foreach (var genreId in request.GenreIds)
            {
                db.MangaGenres.Add(new MangaGenre
                {
                    MangaSeriesId = manga.Id,
                    GenreId = genreId,
                });
            }
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
