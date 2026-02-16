using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using MediatR;

namespace Manga.Application.Comments.Commands.UpdateComment;

/// <summary>
/// Edit a comment's content. Ownership or CommentUpdate permission required.
/// </summary>
[Authorize]
public record UpdateCommentCommand(Guid Id, string Content) : IRequest<Result>;
