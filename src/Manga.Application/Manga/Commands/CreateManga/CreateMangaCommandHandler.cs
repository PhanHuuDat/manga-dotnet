using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Application.Common.Services;
using Manga.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Manga.Commands.CreateManga;

public class CreateMangaCommandHandler(
    IAppDbContext db,
    IAttachmentValidationService attachmentValidator,
    IGenreValidationService genreValidator)
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
        var genreResult = await genreValidator.ValidateAllExistAsync(request.GenreIds, ct);
        if (genreResult is not null) return Result<Guid>.Failure(genreResult.Errors);

        // Validate attachment IDs exist
        var coverResult = await attachmentValidator.ValidateExistsAsync(request.CoverId, "Cover", ct);
        if (coverResult is not null) return Result<Guid>.Failure(coverResult.Errors);

        var bannerResult = await attachmentValidator.ValidateExistsAsync(request.BannerId, "Banner", ct);
        if (bannerResult is not null) return Result<Guid>.Failure(bannerResult.Errors);

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
