using Manga.Application.Bookmarks.DTOs;
using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using MediatR;

namespace Manga.Application.Bookmarks.Commands.ToggleBookmark;

/// <summary>
/// Toggle bookmark for a manga series. Creates if absent, hard-deletes if present.
/// </summary>
[Authorize]
public record ToggleBookmarkCommand(Guid MangaSeriesId) : IRequest<Result<ToggleBookmarkResponse>>;
