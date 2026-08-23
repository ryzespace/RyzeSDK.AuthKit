using Core.KeyManagement.DTO;
using Microsoft.IdentityModel.Tokens;

namespace Core.KeyManagement.Interfaces;

/// <summary>
/// Defines the contract for managing JWT signing keys and their lifecycle.
/// </summary>
/// <remarks>
/// <para>
/// The key store maintains the signing keys used by AuthKit to issue and
/// validate JSON Web Tokens.
/// </para>
/// <para>
/// It provides access to the active signing credentials, lookup of keys
/// by their key identifier (kid), public key discovery through JWKS,
/// and lifecycle operations such as rotation and revocation.
/// </para>
/// <para>
/// Implementations are responsible for securely storing and managing
/// private key material and for ensuring that revoked or otherwise invalid
/// keys are not used to sign new tokens.
/// </para>
/// </remarks>
public interface IJwtKeyStore
{
    /// <summary>
    /// Initializes the key store and loads the persisted signing key state.
    /// </summary>
    /// <remarks>
    /// This method should be called before the key store is used for signing,
    /// validation, rotation, or key discovery operations.
    /// </remarks>
    Task InitializeAsync();

    /// <summary>
    /// Gets the signing credentials for the key currently active for JWT issuance.
    /// </summary>
    /// <returns>The active <see cref="SigningCredentials"/> instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no valid active signing key is available.
    /// </exception>
    SigningCredentials GetActiveSigningCredentials();

    /// <summary>
    /// Gets the signing credentials associated with a specific key identifier.
    /// </summary>
    /// <param name="kid">The unique key identifier kid). </param>
    /// <returns>
    /// The matching <see cref="SigningCredentials"/>, or <c>null</c> when
    /// no key with the specified identifier exists.
    /// </returns>
    SigningCredentials? GetSigningCredentialsByKid(string kid);

    /// <summary>
    /// Gets the public signing keys in JWKS-compatible representation.
    /// </summary>
    /// <returns>
    /// An enumerable collection of <see cref="PublicJwkDto"/> containing
    /// the public key material required to verify JWT signatures.
    /// </returns>
    /// <remarks>
    /// The returned collection must contain public key material only.
    /// Private key material must never be exposed through this method.
    /// </remarks>
    IEnumerable<PublicJwkDto> GetPublicJwks();

    /// <summary>
    /// Rotates the active RSA signing key.
    /// </summary>
    /// <param name="rsaBits">The RSA key size in bits. Defaults to 4096.</param>
    /// <returns>
    /// A task representing the asynchronous rotation operation and containing
    /// metadata for the newly generated signing key.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Rotation creates a new signing key and makes it the active key for
    /// issuing new JWTs.
    /// </para>
    /// <para>
    /// Previously active keys may remain available for signature validation
    /// so that tokens issued before rotation can continue to be validated,
    /// subject to the key lifecycle and retention policy.
    /// </para>
    /// </remarks>
    Task<KeyMetadata> RotateAsync(int rsaBits = 4096);

    /// <summary>
    /// Revokes the signing key associated with specific key identifier.
    /// </summary>
    /// <param name="kid">The unique key identifier (kid) of the key to revoke.</param>
    /// <returns>
    /// A task representing the asynchronous revocation operation and
    /// containing <c>true</c> when the key was successfully revoked;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// A revoked key must not be used to sign new JWTs.
    /// Implementations may retain the public key for validation of
    /// previously issued tokens according to their key retention policy.
    /// </remarks>
    Task<bool> RevokeAsync(string kid);

    /// <summary>
    /// Gets metadata associated with specific key identifier.
    /// </summary>
    /// <param name="kid">The unique key identifier (kid).</param>
    /// <returns>
    /// The corresponding <see cref="KeyMetadata"/>, or <c>null</c> when
    /// no key with the specified identifier exists.
    /// </returns>
    KeyMetadata? GetMetadata(string kid);
}
