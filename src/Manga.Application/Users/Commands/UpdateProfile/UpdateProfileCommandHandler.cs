using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Users.Commands.UpdateProfile;

public class UpdateProfileCommandHandler(
    IAppDbContext db,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateProfileCommand, Result>
{
    public async Task<Result> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        if (!Guid.TryParse(currentUser.UserId, out var userId))
            return Result.Failure("User not authenticated.");

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            return Result.Failure("User not found.");

        user.DisplayName = request.DisplayName;
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
