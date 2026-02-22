using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Admin.Queries.ListUsers;

/// <summary>
/// Returns a paginated, searchable list of users for admin management.
/// Password hash is never projected.
/// </summary>
public class ListUsersQueryHandler(IAppDbContext db)
    : IRequestHandler<ListUsersQuery, Result<PagedResponse<AdminUserDto>>>
{
    private const int MaxPageSize = 100;

    public async Task<Result<PagedResponse<AdminUserDto>>> Handle(
        ListUsersQuery request, CancellationToken ct)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var page = Math.Max(1, request.Page);

        var query = db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term));
        }

        if (request.RoleFilter.HasValue)
        {
            query = query.Where(u =>
                u.UserRoles.Any(r => r.Role == request.RoleFilter.Value));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserDto(
                u.Id,
                u.Username,
                u.Email,
                u.DisplayName,
                u.Avatar != null ? u.Avatar.Url : null,
                u.IsActive,
                u.EmailConfirmed,
                u.UserRoles.Select(r => r.Role.ToString()).ToList(),
                u.CreatedAt))
            .AsNoTracking()
            .ToListAsync(ct);

        return Result<PagedResponse<AdminUserDto>>.Success(
            new PagedResponse<AdminUserDto>(items, page, pageSize, totalCount,
                page * pageSize < totalCount));
    }
}
