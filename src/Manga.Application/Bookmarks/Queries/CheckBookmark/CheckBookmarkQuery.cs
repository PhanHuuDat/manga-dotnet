using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using MediatR;

namespace Manga.Application.Bookmarks.Queries.CheckBookmark;

/// <summary>
/// Check if a manga is bookmarked by the current user.
/// </summary>
[Authorize]
public record CheckBookmarkQuery(Guid MangaSeriesId) : IRequest<Result<bool>>;
