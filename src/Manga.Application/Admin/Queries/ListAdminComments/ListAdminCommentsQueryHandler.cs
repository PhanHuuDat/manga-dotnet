using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Admin.Queries.ListAdminComments;

/// <summary>
/// Returns paginated comments for admin moderation, including soft-deleted records.
/// Uses IgnoreQueryFilters() to bypass the global IsDeleted filter.
/// </summary>
public class ListAdminCommentsQueryHandler(IAppDbContext db)
    : IRequestHandler<ListAdminCommentsQuery, Result<PagedResponse<AdminCommentDto>>>
{
    private const int MaxPageSize = 100;

    public async Task<Result<PagedResponse<AdminCommentDto>>> Handle(
        ListAdminCommentsQuery request, CancellationToken ct)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var page = Math.Max(1, request.Page);

        // IgnoreQueryFilters bypasses the global soft-delete filter on Comment
        var query = db.Comments.IgnoreQueryFilters().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(c => c.Content.ToLower().Contains(term));
        }

        if (request.UserId.HasValue)
            query = query.Where(c => c.UserId == request.UserId.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new AdminCommentDto(
                c.Id,
                c.Content,
                c.User.Username,
                c.UserId,
                c.MangaSeries != null ? c.MangaSeries.Title : null,
                c.CreatedAt,
                c.IsDeleted))
            .AsNoTracking()
            .ToListAsync(ct);

        return Result<PagedResponse<AdminCommentDto>>.Success(
            new PagedResponse<AdminCommentDto>(items, page, pageSize, totalCount,
                page * pageSize < totalCount));
    }
}
