using Manga.Application.Comments.DTOs;
using Manga.Application.Common.Models;
using MediatR;

namespace Manga.Application.Comments.Queries.ListComments;

/// <summary>
/// List comments with threaded replies. Public (no auth required).
/// Filter by manga, chapter, or page.
/// </summary>
public record ListCommentsQuery(
    Guid? MangaSeriesId,
    Guid? ChapterId,
    int? PageNumber,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResponse<CommentDto>>>;
