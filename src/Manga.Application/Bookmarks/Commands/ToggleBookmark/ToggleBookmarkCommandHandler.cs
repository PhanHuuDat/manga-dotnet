using Manga.Application.Bookmarks.DTOs;
using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Bookmarks.Commands.ToggleBookmark;

public class ToggleBookmarkCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<ToggleBookmarkCommand, Result<ToggleBookmarkResponse>>
{
    public async Task<Result<ToggleBookmarkResponse>> Handle(
        ToggleBookmarkCommand request, CancellationToken ct)
    {
        var userId = Guid.Parse(currentUser.UserId!);

        // Validate manga exists
        var mangaExists = await db.MangaSeries
            .AnyAsync(m => m.Id == request.MangaSeriesId, ct);
        if (!mangaExists)
            return Result<ToggleBookmarkResponse>.Failure("Manga series not found.");

        // Check existing bookmark
        var existing = await db.Bookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.MangaSeriesId == request.MangaSeriesId, ct);

        if (existing is not null)
        {
            // Hard delete — bookmarks don't need audit trail
            db.Bookmarks.Remove(existing);
            await db.SaveChangesAsync(ct);
            return Result<ToggleBookmarkResponse>.Success(new(false, null));
        }

        // Create new bookmark
        var bookmark = new Bookmark
        {
            UserId = userId,
            MangaSeriesId = request.MangaSeriesId
        };
        db.Bookmarks.Add(bookmark);
        await db.SaveChangesAsync(ct);

        return Result<ToggleBookmarkResponse>.Success(new(true, bookmark.Id));
    }
}
