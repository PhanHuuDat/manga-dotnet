using System.Security.Cryptography;
using System.Text;

namespace Manga.Application.Common.Helpers;

/// <summary>
/// SHA-256 hashing for refresh/verification tokens before DB storage.
/// </summary>
public static class TokenHasher
{
    public static string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
