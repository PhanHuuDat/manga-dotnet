using Manga.Application.Bookmarks.DTOs;
using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using MediatR;

namespace Manga.Application.Bookmarks.Queries.ListBookmarks;

/// <summary>
/// List current user's bookmarks with manga info, paginated.
/// </summary>
[Authorize]
public record ListBookmarksQuery(int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResponse<BookmarkDto>>>;
