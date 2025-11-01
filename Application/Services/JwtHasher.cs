using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

/// <summary>
/// Utility class for computing cryptographic hashes of JWT tokens.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Uses SHA-256 algorithm to generate a deterministic, fixed-length hash.</item>
/// <item>Provides a secure way to store token references without persisting the full JWT.</item>
/// <item>Ensures constant-time comparison to prevent timing attacks.</item>
/// </list>
/// </remarks>
public static class JwtHasher
{
    /// <summary>
    /// Computes a Base64-encoded SHA256 hash of the given JWT string.
    /// </summary>
    /// <param name="jwt">The raw JWT token to hash.</param>
    /// <returns>Base64-encoded hash of the token.</returns>
    public static string ComputeHash(string jwt)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(jwt);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Compares two JWT hashes using constant-time comparison.
    /// </summary>
    /// <param name="hashA">First hash to compare.</param>
    /// <param name="hashB">Second hash to compare.</param>
    /// <returns><c>true</c> if both hashes are identical; otherwise <c>false</c>.</returns>
    public static bool SecureEquals(string hashA, string hashB)
    {
        if (hashA.Length != hashB.Length)
            return false;

        var result = 0;
        for (int i = 0; i < hashA.Length; i++)
            result |= hashA[i] ^ hashB[i];

        return result == 0;
    }
}