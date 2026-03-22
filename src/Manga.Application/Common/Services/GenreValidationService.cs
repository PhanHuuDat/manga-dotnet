using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Common.Services;

public class GenreValidationService(IAppDbContext db) : IGenreValidationService
{
    public async Task<Result?> ValidateAllExistAsync(
        ICollection<Guid> genreIds, CancellationToken ct)
    {
        if (genreIds.Count == 0)
            return null;

        var existingCount = await db.Genres.CountAsync(g => genreIds.Contains(g.Id), ct);
        return existingCount == genreIds.Count
            ? null
            : Result.Failure("One or more genres not found.");
    }
}
