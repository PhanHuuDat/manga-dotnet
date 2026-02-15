using Manga.Application.Common.Helpers;
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
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IEmailService emailService) : IRequestHandler<RegisterCommand, Result>
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
            EmailConfirmed = false,
        };

        db.Users.Add(user);
        db.UserRoleMappings.Add(new UserRoleMapping { UserId = user.Id, Role = UserRole.Reader });

        // Generate email verification token
        var rawToken = tokenService.GenerateEmailToken();
        db.VerificationTokens.Add(new VerificationToken
        {
            UserId = user.Id,
            Token = TokenHasher.Hash(rawToken),
            TokenType = VerificationTokenType.EmailVerification,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
        });

        await db.SaveChangesAsync(ct);
        await emailService.SendEmailVerificationAsync(user.Email, user.Username, rawToken, user.Id, ct);

        return Result.Success();
    }
}
