using Manga.Infrastructure.Auth;

namespace Manga.Infrastructure.Tests.Auth;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_ReturnsDifferentHashEachTime()
    {
        var hash1 = _hasher.Hash("TestPassword123!");
        var hash2 = _hasher.Hash("TestPassword123!");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var password = "SecurePassword123!";
        var hash = _hasher.Hash(password);

        Assert.True(_hasher.Verify(password, hash));
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("CorrectPassword123!");

        Assert.False(_hasher.Verify("WrongPassword123!", hash));
    }

    [Fact]
    public void Hash_ReturnsNonEmptyString()
    {
        var hash = _hasher.Hash("AnyPassword");

        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }
}
