using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Manga.Commands.UpdateManga;

/// <summary>
/// Updates an existing manga series. Requires MangaUpdate permission.
/// Uploaders can only update their own manga; Moderators/Admins can update any.
/// </summary>
[RequirePermission(nameof(Permission.MangaUpdate))]
public record UpdateMangaCommand(
    Guid Id,
    string? Title,
    string? Synopsis,
    Guid? ArtistId,
    List<Guid>? GenreIds,
    SeriesStatus? Status,
    MangaBadge? Badge,
    int? PublishedYear,
    Guid? CoverId,
    Guid? BannerId) : IRequest<Result>;
