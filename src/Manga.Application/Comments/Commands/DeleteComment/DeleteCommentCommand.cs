using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using MediatR;

namespace Manga.Application.Comments.Commands.DeleteComment;

/// <summary>
/// Soft-delete a comment. Ownership or CommentDelete/Moderate permission required.
/// </summary>
[Authorize]
public record DeleteCommentCommand(Guid Id) : IRequest<Result>;
