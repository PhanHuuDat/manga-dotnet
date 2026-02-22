using Manga.Application.Auth.DTOs;
using Manga.Application.Common.Helpers;
using Manga.Application.Common.Interfaces;
using Manga.Application.Common.Models;
using Manga.Domain.Constants;
using Manga.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Manga.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IAppDbContext db,
    ITokenService tokenService,
    IAuthSettings authSettings) : IRequestHandler<RefreshTokenCommand, Result<LoginTokenResult>>
{
    public async Task<Result<LoginTokenResult>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hashedToken = TokenHasher.Hash(request.RawRefreshToken);

        var storedToken = await db.RefreshTokens
            .Include(t => t.User).ThenInclude(u => u.UserRoles)
            .FirstOrDefaultAsync(t => t.Token == hashedToken, ct);

        if (storedToken is null)
            return Result<LoginTokenResult>.Failure("Invalid refresh token.");

        // Reuse detection: if token already revoked, revoke entire family
        if (storedToken.IsRevoked)
        {
            var familyTokens = await db.RefreshTokens
                .Where(t => t.Family == storedToken.Family && t.RevokedAt == null)
                .ToListAsync(ct);

            foreach (var ft in familyTokens)
                ft.RevokedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync(ct);
            return Result<LoginTokenResult>.Failure("Token reuse detected. All sessions revoked.");
        }

        if (storedToken.IsExpired)
            return Result<LoginTokenResult>.Failure("Refresh token expired.");

        if (!storedToken.User.IsActive)
            return Result<LoginTokenResult>.Failure("Account has been deactivated.");

        // Revoke current token
        storedToken.RevokedAt = DateTimeOffset.UtcNow;

        // Generate new tokens
        var user = storedToken.User;
        var roles = user.UserRoles.Select(r => r.Role).ToList();
        var roleNames = roles.Select(r => r.ToString()).ToList();
        var permissions = RolePermissions.GetPermissions(roles).Select(p => p.ToString()).ToList();

        var (accessToken, jti, expiresAt) = tokenService.GenerateAccessToken(
            user.Id, user.Username, roleNames, permissions);

        var rawNewRefresh = tokenService.GenerateRefreshToken();
        var refreshExpiry = DateTimeOffset.UtcNow.AddDays(authSettings.RefreshTokenExpirationDays);

        var newRefreshToken = new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = TokenHasher.Hash(rawNewRefresh),
            Family = storedToken.Family,
            ExpiresAt = refreshExpiry,
        };

        db.RefreshTokens.Add(newRefreshToken);
        storedToken.ReplacedByTokenId = newRefreshToken.Id;

        await db.SaveChangesAsync(ct);

        var authResponse = new AuthResponse(accessToken, expiresAt, user.Id.ToString(), user.Username, user.DisplayName);
        return Result<LoginTokenResult>.Success(new LoginTokenResult(authResponse, rawNewRefresh, refreshExpiry));
    }
}
