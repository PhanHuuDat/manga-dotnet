using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Manga.Commands.CreateManga;

/// <summary>
/// Creates a new manga series with genres. Requires MangaCreate permission.
/// </summary>
[RequirePermission(nameof(Permission.MangaCreate))]
public record CreateMangaCommand(
    string Title,
    string? Synopsis,
    Guid AuthorId,
    Guid? ArtistId,
    List<Guid> GenreIds,
    SeriesStatus Status,
    int? PublishedYear,
    Guid? CoverId,
    Guid? BannerId) : IRequest<Result<Guid>>;
