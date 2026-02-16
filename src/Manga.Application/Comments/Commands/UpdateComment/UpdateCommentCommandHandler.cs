using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Comments.Commands.UpdateComment;

public class UpdateCommentCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<UpdateCommentCommand, Result>
{
    public async Task<Result> Handle(UpdateCommentCommand request, CancellationToken ct)
    {
        var userId = Guid.Parse(currentUser.UserId!);

        var comment = await db.Comments.FirstOrDefaultAsync(c => c.Id == request.Id, ct);
        if (comment is null)
            return Result.Failure("Comment not found.");

        // Ownership check (permission check handled by AuthorizationBehavior for moderators)
        if (comment.UserId != userId)
            return Result.Failure("Cannot edit another user's comment.");

        comment.Content = request.Content;
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
