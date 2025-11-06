using Application.DTO.Key;
using Microsoft.IdentityModel.Tokens;

namespace Application.Interfaces.JWTKey;

/// <summary>
/// Defines a contract for managing JWT signing keys with lifecycle operations such as rotation and revocation.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Maintains the active signing key used for issuing JWT tokens.</item>
/// <item>Supports key lookup by KID (Key Identifier) for signature validation.</item>
/// <item>Exposes a JWKS endpoint-compatible collection of public keys.</item>
/// <item>Provides secure key rotation and revocation management.</item>
/// </list>
/// </remarks>
public interface IJwtKeyStore
{
    /// <summary>
    /// Retrieves the currently active signing credentials used for JWT issuance.
    /// </summary>
    /// <returns>The active <see cref="SigningCredentials"/> instance.</returns>
    SigningCredentials GetActiveSigningCredentials();

    /// <summary>
    /// Retrieves signing credentials for a specific key identifier (KID).
    /// </summary>
    /// <param name="kid">The unique key identifier.</param>
    /// <returns>The matching <see cref="SigningCredentials"/> or <c>null</c> if not found.</returns>
    SigningCredentials? GetSigningCredentialsByKid(string kid);

    /// <summary>
    /// Returns the collection of public keys in JWKS (JSON Web Key Set) format.
    /// </summary>
    /// <returns>An enumerable of <see cref="JsonWebKey"/> representing the public keys.</returns>
    IEnumerable<JsonWebKey> GetPublicJwks();

    /// <summary>
    /// Rotates the active RSA signing key and returns metadata for the newly generated key.
    /// </summary>
    /// <param name="rsaBits">The RSA key size in bits (default is 4096).</param>
    /// <returns>A <see cref="KeyMetadata"/> object describing the new key.</returns>
    KeyMetadata Rotate(int rsaBits = 4096);

    /// <summary>
    /// Revokes the signing key associated with the specified key identifier (KID).
    /// </summary>
    /// <param name="kid">The unique key identifier of the key to revoke.</param>
    /// <returns><c>true</c> if the key was successfully revoked; otherwise, <c>false</c>.</returns>
    bool Revoke(string kid);

    /// <summary>
    /// Retrieves metadata for a specific key identifier (KID).
    /// </summary>
    /// <param name="kid">The unique key identifier.</param>
    /// <returns>The <see cref="KeyMetadata"/> of the key, or <c>null</c> if not found.</returns>
    KeyMetadata? GetMetadata(string kid);
}
