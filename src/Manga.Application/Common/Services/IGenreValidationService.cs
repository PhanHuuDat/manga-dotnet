using Manga.Application.Common.Models;

namespace Manga.Application.Common.Services;

public interface IGenreValidationService
{
    /// <summary>Validates all genre IDs exist. Returns failure Result if any missing.</summary>
    Task<Result?> ValidateAllExistAsync(ICollection<Guid> genreIds, CancellationToken ct);
}
