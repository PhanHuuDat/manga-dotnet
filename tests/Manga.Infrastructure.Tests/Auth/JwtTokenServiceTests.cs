using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Manga.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace Manga.Infrastructure.Tests.Auth;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;

    public JwtTokenServiceTests()
    {
        var settings = Options.Create(new JwtSettings
        {
            Secret = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly123!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7,
        });
        _service = new JwtTokenService(settings);
    }

    [Fact]
    public void GenerateAccessToken_ReturnsValidJwt_WithCorrectClaims()
    {
        var userId = Guid.NewGuid();
        var username = "testuser";
        var roles = new[] { "Reader", "Admin" };
        var permissions = new[] { "MangaCreate", "ChapterCreate" };

        var (token, jti, expiresAt) = _service.GenerateAccessToken(userId, username, roles, permissions);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(userId.ToString(), jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(username, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value);
        Assert.Equal(jti, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value);
        Assert.Equal(2, jwt.Claims.Count(c => c.Type == ClaimTypes.Role));
        Assert.Equal(2, jwt.Claims.Count(c => c.Type == "permission"));
    }

    [Fact]
    public void GenerateAccessToken_ExpiresAt_MatchesConfig()
    {
        var (_, _, expiresAt) = _service.GenerateAccessToken(
            Guid.NewGuid(), "user", ["Reader"], ["CommentCreate"]);

        var expectedExpiry = DateTimeOffset.UtcNow.AddMinutes(15);
        Assert.InRange(expiresAt, expectedExpiry.AddSeconds(-5), expectedExpiry.AddSeconds(5));
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsBase64String()
    {
        var token = _service.GenerateRefreshToken();

        Assert.NotNull(token);
        Assert.NotEmpty(token);
        // 64 bytes → 88 base64 chars (with padding)
        var bytes = Convert.FromBase64String(token);
        Assert.Equal(64, bytes.Length);
    }

    [Fact]
    public void GenerateEmailToken_ReturnsUrlSafeString()
    {
        var token = _service.GenerateEmailToken();

        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.DoesNotContain("+", token);
        Assert.DoesNotContain("/", token);
        Assert.DoesNotContain("=", token);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsDifferentTokensEachCall()
    {
        var token1 = _service.GenerateRefreshToken();
        var token2 = _service.GenerateRefreshToken();

        Assert.NotEqual(token1, token2);
    }
}
