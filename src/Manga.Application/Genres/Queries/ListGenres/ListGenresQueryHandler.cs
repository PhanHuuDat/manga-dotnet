using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Application.Genres.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Manga.Application.Genres.Queries.ListGenres;

public class ListGenresQueryHandler(IAppDbContext db, IMemoryCache cache)
    : IRequestHandler<ListGenresQuery, Result<IReadOnlyList<GenreWithCountDto>>>
{
    private const string CacheKey = "genres_list";

    public async Task<Result<IReadOnlyList<GenreWithCountDto>>> Handle(
        ListGenresQuery request, CancellationToken ct)
    {
        if (cache.TryGetValue(CacheKey, out IReadOnlyList<GenreWithCountDto>? genres))
            return Result<IReadOnlyList<GenreWithCountDto>>.Success(genres!);

        genres = await db.Genres
            .Select(g => new GenreWithCountDto(
                g.Id,
                g.Name,
                g.Slug,
                g.Description,
                g.MangaGenres.Count(mg => !mg.MangaSeries.IsDeleted)))
            .OrderBy(g => g.Name)
            .ToListAsync(ct);

        cache.Set(CacheKey, genres, TimeSpan.FromHours(24));

        return Result<IReadOnlyList<GenreWithCountDto>>.Success(genres);
    }
}
