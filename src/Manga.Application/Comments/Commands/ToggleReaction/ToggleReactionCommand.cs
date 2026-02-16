using Manga.Application.Comments.DTOs;
using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Comments.Commands.ToggleReaction;

/// <summary>
/// Toggle a like/dislike reaction on a comment.
/// </summary>
[Authorize]
public record ToggleReactionCommand(Guid CommentId, ReactionType ReactionType)
    : IRequest<Result<ToggleReactionResponse>>;
