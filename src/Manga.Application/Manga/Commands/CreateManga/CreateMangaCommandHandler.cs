using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Manga.Commands.CreateManga;

public class CreateMangaCommandHandler(IAppDbContext db)
    : IRequestHandler<CreateMangaCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMangaCommand request, CancellationToken ct)
    {
        // Validate author exists
        if (!await db.Persons.AnyAsync(p => p.Id == request.AuthorId, ct))
            return Result<Guid>.Failure("Author not found.");

        // Validate artist exists (if provided)
        if (request.ArtistId.HasValue &&
            !await db.Persons.AnyAsync(p => p.Id == request.ArtistId.Value, ct))
            return Result<Guid>.Failure("Artist not found.");

        // Validate all genre IDs exist
        var existingGenreCount = await db.Genres
            .CountAsync(g => request.GenreIds.Contains(g.Id), ct);
        if (existingGenreCount != request.GenreIds.Count)
            return Result<Guid>.Failure("One or more genres not found.");

        // Validate attachment IDs exist
        if (request.CoverId.HasValue &&
            !await db.Attachments.AnyAsync(a => a.Id == request.CoverId.Value, ct))
            return Result<Guid>.Failure("Cover attachment not found.");

        if (request.BannerId.HasValue &&
            !await db.Attachments.AnyAsync(a => a.Id == request.BannerId.Value, ct))
            return Result<Guid>.Failure("Banner attachment not found.");

        var manga = new MangaSeries
        {
            Title = request.Title,
            Synopsis = request.Synopsis,
            AuthorId = request.AuthorId,
            ArtistId = request.ArtistId,
            Status = request.Status,
            PublishedYear = request.PublishedYear,
            CoverId = request.CoverId,
            BannerId = request.BannerId,
        };

        db.MangaSeries.Add(manga);

        foreach (var genreId in request.GenreIds)
        {
            db.MangaGenres.Add(new MangaGenre
            {
                MangaSeriesId = manga.Id,
                GenreId = genreId,
            });
        }

        await db.SaveChangesAsync(ct);
        return Result<Guid>.Success(manga.Id);
    }
}
