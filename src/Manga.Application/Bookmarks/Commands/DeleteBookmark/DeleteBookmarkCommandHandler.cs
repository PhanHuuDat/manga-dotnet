using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Bookmarks.Commands.DeleteBookmark;

public class DeleteBookmarkCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<DeleteBookmarkCommand, Result>
{
    public async Task<Result> Handle(DeleteBookmarkCommand request, CancellationToken ct)
    {
        var userId = Guid.Parse(currentUser.UserId!);

        var bookmark = await db.Bookmarks
            .FirstOrDefaultAsync(b => b.Id == request.Id, ct);

        if (bookmark is null)
            return Result.Failure("Bookmark not found.");

        if (bookmark.UserId != userId)
            return Result.Failure("Cannot delete another user's bookmark.");

        db.Bookmarks.Remove(bookmark);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
