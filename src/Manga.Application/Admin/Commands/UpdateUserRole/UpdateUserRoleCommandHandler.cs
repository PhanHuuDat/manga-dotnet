using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Admin.Commands.UpdateUserRole;

/// <summary>
/// Grants or revokes a role mapping. Prevents removing own admin role
/// and removing the last admin from the platform.
/// </summary>
public class UpdateUserRoleCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateUserRoleCommand, Result>
{
    public async Task<Result> Handle(UpdateUserRoleCommand request, CancellationToken ct)
    {
        if (!Guid.TryParse(currentUser.UserId, out var callerId))
            return Result.Failure("User not authenticated.");

        if (request.UserId == callerId)
            return Result.Failure("Cannot change your own role.");

        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

        if (user is null)
            return Result.Failure("User not found.");

        var existing = user.UserRoles.FirstOrDefault(r => r.Role == request.Role);

        if (!request.Grant)
        {
            // Prevent removing the last admin
            if (request.Role == UserRole.Admin)
            {
                var adminCount = await db.UserRoleMappings
                    .CountAsync(r => r.Role == UserRole.Admin, ct);

                if (adminCount <= 1)
                    return Result.Failure("Cannot remove the last admin.");
            }

            if (existing is not null)
                db.UserRoleMappings.Remove(existing);
        }
        else
        {
            if (existing is null)
                db.UserRoleMappings.Add(new UserRoleMapping
                {
                    UserId = request.UserId,
                    Role = request.Role,
                });
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
