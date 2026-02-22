using Manga.Application.Common.Models;
using Manga.Application.Common.Security;
using Manga.Domain.Enums;
using MediatR;

namespace Manga.Application.Admin.Queries.ListAdminComments;

/// <summary>
/// Paginated comment list for admin moderation, including soft-deleted. Requires AdminManageComments permission.
/// </summary>
[RequirePermission(nameof(Permission.AdminManageComments))]
public record ListAdminCommentsQuery(
    int Page,
    int PageSize,
    string? Search,
    Guid? UserId) : IRequest<Result<PagedResponse<AdminCommentDto>>>;
