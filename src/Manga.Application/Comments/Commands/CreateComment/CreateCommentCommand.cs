using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Comments.Commands.CreateComment;

/// <summary>
/// Create a comment on a manga, chapter, or page. Optionally as a reply.
/// </summary>
[RequirePermission(nameof(Permission.CommentCreate))]
public record CreateCommentCommand(
    string Content,
    Guid? MangaSeriesId,
    Guid? ChapterId,
    int? PageNumber,
    Guid? ParentId) : IRequest<Result<Guid>>;
