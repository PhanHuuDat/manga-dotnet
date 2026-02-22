using Manga.Application.Auth.DTOs;
using Manga.Application.Common.Helpers;
using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Constants;
using Manga.Domain.Interfaces;
using RefreshTokenEntity = Manga.Domain.Entities.RefreshToken;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Auth.Commands.Login;

public class LoginCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IAuthSettings authSettings) : IRequestHandler<LoginCommand, Result<LoginTokenResult>>
{
    public async Task<Result<LoginTokenResult>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<LoginTokenResult>.Failure("Invalid credentials.");

        if (!user.IsActive)
            return Result<LoginTokenResult>.Failure("Account has been deactivated.");

        var roles = user.UserRoles.Select(r => r.Role).ToList();
        var roleNames = roles.Select(r => r.ToString()).ToList();
        var permissions = RolePermissions.GetPermissions(roles)
            .Select(p => p.ToString()).ToList();

        var (accessToken, jti, expiresAt) = tokenService.GenerateAccessToken(
            user.Id, user.Username, roleNames, permissions);

        var rawRefreshToken = tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(authSettings.RefreshTokenExpirationDays);

        db.RefreshTokens.Add(new RefreshTokenEntity
        {
            UserId = user.Id,
            Token = TokenHasher.Hash(rawRefreshToken),
            Family = Guid.NewGuid(),
            ExpiresAt = refreshTokenExpiry,
        });

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        var authResponse = new AuthResponse(accessToken, expiresAt, user.Id.ToString(), user.Username, user.DisplayName);
        return Result<LoginTokenResult>.Success(new LoginTokenResult(authResponse, rawRefreshToken, refreshTokenExpiry));
    }
}
