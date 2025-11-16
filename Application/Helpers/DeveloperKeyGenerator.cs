using System.Security.Cryptography;
using System.Text;

namespace Application.Helpers;

/// <summary>
/// Helper class for generating developer-friendly keys from JWT hashes.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Generates short, alphanumeric keys suitable for developer usage in API headers.</item>
/// <item>Transforms SHA256 JWT hashes into Base32-friendly strings.</item>
/// <item>Supports optional prefix and configurable length.</item>
/// </list>
/// </remarks>
public static class DeveloperKeyGenerator
{
    /// <summary>
    /// Generates a developer-friendly short key from a full JWT hash.
    /// </summary>
    /// <param name="fullHash">Full SHA256 Base64 hash of the JWT token.</param>
    /// <param name="length">Length of the short key (default: 16 characters).</param>
    /// <param name="prefix">Optional prefix for the short key (e.g., "dev_").</param>
    /// <returns>Short alphanumeric key suitable for API usage.</returns>
    public static string GenerateShortKey(string fullHash, int length = 16, string? prefix = null)
    {
        if (string.IsNullOrWhiteSpace(fullHash))
            throw new ArgumentException("Full hash cannot be null or empty.", nameof(fullHash));

        var base32 = Convert.ToBase64String(Encoding.UTF8.GetBytes(fullHash))
            .Replace("/", "")
            .Replace("+", "")
            .Replace("=", "");

        var key = base32.Substring(0, Math.Min(length, base32.Length));

        return string.IsNullOrEmpty(prefix) ? key : $"{prefix}{key}";
    }

    /// <summary>
    /// Generates a random alphanumeric key of a given length, useful for initial tokens.
    /// </summary>
    /// <param name="length">Length of the key (default: 16).</param>
    /// <param name="prefix">Optional prefix.</param>
    /// <returns>Random alphanumeric string.</returns>
    public static string GenerateRandomKey(int length = 16, string? prefix = null)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var sb = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            var idx = RandomNumberGenerator.GetInt32(0, chars.Length);
            sb.Append(chars[idx]);
        }

        return string.IsNullOrEmpty(prefix) ? sb.ToString() : $"{prefix}{sb}";
    }
}