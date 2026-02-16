using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Application.Manga.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Manga.Queries.GetManga;

public class GetMangaQueryHandler(IAppDbContext db)
    : IRequestHandler<GetMangaQuery, Result<MangaDetailDto>>
{
    public async Task<Result<MangaDetailDto>> Handle(GetMangaQuery request, CancellationToken ct)
    {
        var manga = await db.MangaSeries
            .Include(m => m.Author).ThenInclude(a => a.Photo)
            .Include(m => m.Artist!).ThenInclude(a => a!.Photo)
            .Include(m => m.Cover)
            .Include(m => m.Banner)
            .Include(m => m.MangaGenres).ThenInclude(mg => mg.Genre)
            .Include(m => m.AlternativeTitles)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.Id, ct);

        if (manga is null)
            return Result<MangaDetailDto>.Failure("Manga not found.");

        var dto = new MangaDetailDto(
            manga.Id,
            manga.Title,
            manga.Synopsis,
            manga.Cover?.Url,
            manga.Banner?.Url,
            new PersonDto(manga.Author.Id, manga.Author.Name, manga.Author.Biography, manga.Author.Photo?.Url),
            manga.Artist is not null
                ? new PersonDto(manga.Artist.Id, manga.Artist.Name, manga.Artist.Biography, manga.Artist.Photo?.Url)
                : null,
            manga.MangaGenres.Select(mg => new GenreDto(mg.Genre.Id, mg.Genre.Name, mg.Genre.Slug, mg.Genre.Description)).ToList(),
            manga.AlternativeTitles.Select(at => at.Title).ToList(),
            manga.Status,
            manga.Badge,
            manga.PublishedYear,
            manga.Rating,
            manga.RatingCount,
            manga.Views,
            manga.TotalChapters,
            manga.LatestChapterNumber,
            manga.CreatedAt);

        return Result<MangaDetailDto>.Success(dto);
    }
}
