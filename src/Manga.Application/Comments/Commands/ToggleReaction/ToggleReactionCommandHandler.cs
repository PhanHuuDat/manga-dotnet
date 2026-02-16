using Manga.Application.Comments.DTOs;
using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Comments.Commands.ToggleReaction;

public class ToggleReactionCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<ToggleReactionCommand, Result<ToggleReactionResponse>>
{
    public async Task<Result<ToggleReactionResponse>> Handle(
        ToggleReactionCommand request, CancellationToken ct)
    {
        var userId = Guid.Parse(currentUser.UserId!);

        var comment = await db.Comments.FirstOrDefaultAsync(c => c.Id == request.CommentId, ct);
        if (comment is null)
            return Result<ToggleReactionResponse>.Failure("Comment not found.");

        var existing = await db.CommentReactions
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CommentId == request.CommentId, ct);

        if (existing is not null)
        {
            if (existing.ReactionType == request.ReactionType)
            {
                // Toggle off — remove reaction
                db.CommentReactions.Remove(existing);
                AdjustCounter(comment, existing.ReactionType, -1);
                await db.SaveChangesAsync(ct);
                return Result<ToggleReactionResponse>.Success(
                    new(null, comment.Likes, comment.Dislikes));
            }

            // Switch reaction type
            AdjustCounter(comment, existing.ReactionType, -1);
            existing.ReactionType = request.ReactionType;
            AdjustCounter(comment, request.ReactionType, 1);
            await db.SaveChangesAsync(ct);
            return Result<ToggleReactionResponse>.Success(
                new(request.ReactionType, comment.Likes, comment.Dislikes));
        }

        // New reaction
        db.CommentReactions.Add(new CommentReaction
        {
            UserId = userId,
            CommentId = request.CommentId,
            ReactionType = request.ReactionType,
        });
        AdjustCounter(comment, request.ReactionType, 1);
        await db.SaveChangesAsync(ct);

        return Result<ToggleReactionResponse>.Success(
            new(request.ReactionType, comment.Likes, comment.Dislikes));
    }

    private static void AdjustCounter(Comment comment, ReactionType type, int delta)
    {
        if (type == ReactionType.Like)
            comment.Likes = Math.Max(0, comment.Likes + delta);
        else
            comment.Dislikes = Math.Max(0, comment.Dislikes + delta);
    }
}
