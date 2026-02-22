using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Admin.Queries.ListUsers;

/// <summary>
/// Paginated list of users with optional search and role filter. Requires AdminManageUsers permission.
/// </summary>
[RequirePermission(nameof(Permission.AdminManageUsers))]
public record ListUsersQuery(
    int Page,
    int PageSize,
    string? Search,
    UserRole? RoleFilter) : IRequest<Result<PagedResponse<AdminUserDto>>>;
