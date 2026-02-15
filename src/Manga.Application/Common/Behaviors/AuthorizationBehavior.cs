using System.Reflection;
using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Security;
using Manga.Domain.Constants;
using Manga.Domain.Enums;
using Manga.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that enforces [Authorize] and [RequirePermission] attributes.
/// Runs BEFORE ValidationBehavior.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse>(
    ICurrentUserService currentUser,
    IAppDbContext db) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var authorizeAttributes = typeof(TRequest)
            .GetCustomAttributes<AuthorizeAttribute>(true)
            .ToList();

        if (authorizeAttributes.Count == 0)
            return await next(ct);

        // Must be authenticated
        if (string.IsNullOrEmpty(currentUser.UserId))
            throw new UnauthorizedAccessException();

        // Check permission attributes
        var permissionAttributes = authorizeAttributes.OfType<RequirePermissionAttribute>().ToList();
        if (permissionAttributes.Count == 0)
            return await next(ct);

        var userId = Guid.Parse(currentUser.UserId);
        var userRoles = await db.UserRoleMappings
            .Where(r => r.UserId == userId)
            .Select(r => r.Role)
            .ToListAsync(ct);

        var userPermissions = RolePermissions.GetPermissions(userRoles)
            .Select(p => p.ToString())
            .ToHashSet();

        foreach (var attr in permissionAttributes)
        {
            if (!userPermissions.Contains(attr.Permission))
                throw new ForbiddenAccessException();
        }

        return await next(ct);
    }
}
