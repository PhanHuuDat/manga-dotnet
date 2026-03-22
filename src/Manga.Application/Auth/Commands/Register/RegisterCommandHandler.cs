using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Entities;
using Manga.Domain.Enums;
using Manga.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Auth.Commands.Register;

public class RegisterCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher) : IRequestHandler<RegisterCommand, Result>
{
    public async Task<Result> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Username == request.Username, ct))
            return Result.Failure("Username is already taken.");

        if (await db.Users.AnyAsync(u => u.Email == request.Email, ct))
            return Result.Failure("Email is already registered.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            // Email verification disabled temporarily — auto-confirm on registration
            EmailConfirmed = true,
        };

        db.Users.Add(user);
        db.UserRoleMappings.Add(new UserRoleMapping { UserId = user.Id, Role = UserRole.Reader });

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
