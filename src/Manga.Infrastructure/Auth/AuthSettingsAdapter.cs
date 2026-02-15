using Manga.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Manga.Infrastructure.Auth;

/// <summary>
/// Adapts JwtSettings to IAuthSettings for clean architecture boundary.
/// </summary>
public class AuthSettingsAdapter(IOptions<JwtSettings> settings) : IAuthSettings
{
    public int RefreshTokenExpirationDays => settings.Value.RefreshTokenExpirationDays;
}
