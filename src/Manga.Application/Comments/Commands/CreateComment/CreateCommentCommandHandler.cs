using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Comments.Commands.CreateComment;

public class CreateCommentCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<CreateCommentCommand, Result<Guid>>
{
    private const int MaxReplyDepth = 3;

    public async Task<Result<Guid>> Handle(CreateCommentCommand request, CancellationToken ct)
    {
        var userId = Guid.Parse(currentUser.UserId!);

        // Validate targets exist
        if (request.MangaSeriesId.HasValue &&
            !await db.MangaSeries.AnyAsync(m => m.Id == request.MangaSeriesId, ct))
            return Result<Guid>.Failure("Manga series not found.");

        if (request.ChapterId.HasValue &&
            !await db.Chapters.AnyAsync(c => c.Id == request.ChapterId, ct))
            return Result<Guid>.Failure("Chapter not found.");

        // Validate reply depth — single query loads parent + 2 ancestor levels
        if (request.ParentId.HasValue)
        {
            var parent = await db.Comments
                .Include(c => c.Parent)
                    .ThenInclude(c => c!.Parent)
                .FirstOrDefaultAsync(c => c.Id == request.ParentId, ct);

            if (parent is null)
                return Result<Guid>.Failure("Parent comment not found.");

            // Calculate depth in-memory (0 = direct child of root)
            var depth = 0;
            var current = parent;
            while (current.ParentId.HasValue && current.Parent is not null)
            {
                depth++;
                current = current.Parent;
            }
            if (depth + 1 > MaxReplyDepth)
                return Result<Guid>.Failure($"Maximum reply depth ({MaxReplyDepth}) reached.");

            parent.ReplyCount++;
        }

        var comment = new Comment
        {
            UserId = userId,
            Content = request.Content,
            MangaSeriesId = request.MangaSeriesId,
            ChapterId = request.ChapterId,
            PageNumber = request.PageNumber,
            ParentId = request.ParentId,
        };

        db.Comments.Add(comment);
        await db.SaveChangesAsync(ct);

        return Result<Guid>.Success(comment.Id);
    }
}
