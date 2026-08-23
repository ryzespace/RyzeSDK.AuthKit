using Microsoft.IdentityModel.Tokens;

namespace Core.KeyManagement.DTO;

/// <summary>
/// Represents single RSA key entry used for JWT signing.
/// </summary>
/// <remarks>
/// <para>
/// A key entry combines the RSA security key used for cryptographic operations,
/// the signing credentials used for JWT issuance, associated key metadata,
/// and the public RSA parameters required for JWKS representation.
/// </para>
/// <para>
/// The RSA modulus and exponent are exposed as encoded strings so that the
/// public key material can be published through a JWKS endpoint without
/// exposing private RSA parameters.
/// </para>
/// </remarks>
public sealed record KeyEntry
{
    /// <summary>
    /// Gets the RSA security key used for JWT signing operations.
    /// </summary>
    public required RsaSecurityKey Key { get; init; }

    /// <summary>
    /// Gets the signing credentials used when issuing JWTs with this key.
    /// </summary>
    public required SigningCredentials Signing { get; init; }

    /// <summary>
    /// Gets the metadata associated with the RSA key.
    /// </summary>
    public required KeyMetadata Meta { get; init; }

    /// <summary>
    /// Gets the Base64URL-encoded RSA modulus.
    /// </summary>
    /// <remarks>
    /// The modulus corresponds to the <c>n</c> parameter defined by the
    /// JSON Web Key (JWK) specification and is used when exporting the
    /// public key as JWKS.
    /// </remarks>
    public required string N { get; init; }

    /// <summary>
    /// Gets the Base64URL-encoded RSA public exponent.
    /// </summary>
    /// <remarks>
    /// The exponent corresponds to the <c>e</c> parameter defined by the
    /// JSON Web Key (JWK) specification and is used when exporting the
    /// public key as JWKS.
    /// </remarks>
    public required string E { get; init; }
}