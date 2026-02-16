using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using MediatR;

namespace Manga.Application.Bookmarks.Commands.DeleteBookmark;

/// <summary>
/// Delete a bookmark by ID (ownership validated in handler).
/// </summary>
[Authorize]
public record DeleteBookmarkCommand(Guid Id) : IRequest<Result>;
