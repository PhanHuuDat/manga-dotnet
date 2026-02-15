namespace Manga.Domain.Interfaces;

/// <summary>
/// Password hashing contract. Implementation in Infrastructure (BCrypt).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
