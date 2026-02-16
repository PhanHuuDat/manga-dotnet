using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Comments.Commands.DeleteComment;

public class DeleteCommentCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<DeleteCommentCommand, Result>
{
    public async Task<Result> Handle(DeleteCommentCommand request, CancellationToken ct)
    {
        var userId = Guid.Parse(currentUser.UserId!);

        var comment = await db.Comments.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
        if (comment is null)
            return Result.Failure("Comment not found.");

        // Ownership check — moderators handled via permission in endpoint-level auth
        if (comment.UserId != userId)
            return Result.Failure("Cannot delete another user's comment.");

        // Soft-delete (AuditableEntity)
        comment.IsDeleted = true;

        // Decrement parent ReplyCount if this is a reply
        if (comment.ParentId.HasValue)
        {
            var parent = await db.Comments.FirstOrDefaultAsync(c => c.Id == comment.ParentId, ct);
            if (parent is not null)
                parent.ReplyCount = Math.Max(0, parent.ReplyCount - 1);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
