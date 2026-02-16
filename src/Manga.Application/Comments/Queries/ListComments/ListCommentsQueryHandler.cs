using Manga.Application.Comments.DTOs;
using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Comments.Queries.ListComments;

public class ListCommentsQueryHandler(IAppDbContext db)
    : IRequestHandler<ListCommentsQuery, Result<PagedResponse<CommentDto>>>
{
    public async Task<Result<PagedResponse<CommentDto>>> Handle(
        ListCommentsQuery request, CancellationToken ct)
    {
        var query = db.Comments
            .Where(c => c.ParentId == null); // Top-level only

        // Apply filters
        if (request.MangaSeriesId.HasValue)
            query = query.Where(c => c.MangaSeriesId == request.MangaSeriesId);
        if (request.ChapterId.HasValue)
            query = query.Where(c => c.ChapterId == request.ChapterId);
        if (request.PageNumber.HasValue)
            query = query.Where(c => c.PageNumber == request.PageNumber);

        var totalCount = await query.CountAsync(ct);

        // Fetch with replies up to 3 levels deep + user info at each level
        var comments = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(c => c.User).ThenInclude(u => u.Avatar)
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.User).ThenInclude(u => u.Avatar)
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.Replies.Where(rr => !rr.IsDeleted))
                    .ThenInclude(rr => rr.User).ThenInclude(u => u.Avatar)
            .ToListAsync(ct);

        var items = comments.Select(MapToDto).ToList();

        var hasNext = request.Page * request.PageSize < totalCount;
        return Result<PagedResponse<CommentDto>>.Success(
            new(items, request.Page, request.PageSize, totalCount, hasNext));
    }

    private static CommentDto MapToDto(Comment c) => new(
        c.Id, c.UserId, c.User.Username, c.User.Avatar?.Url,
        c.Content, c.Likes, c.Dislikes,
        c.MangaSeriesId, c.ChapterId, c.PageNumber,
        c.ParentId, c.ReplyCount,
        c.Replies.Where(r => !r.IsDeleted).Select(MapToDto).ToList(),
        c.CreatedAt, c.LastModifiedAt);
}
