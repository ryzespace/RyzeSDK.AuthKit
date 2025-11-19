using Microsoft.IdentityModel.Tokens;

namespace Application.Features.KeyManagement.DTO;

/// <summary>
/// Represents a single RSA key entry used for JWT signing.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Holds the <see cref="RsaSecurityKey"/> instance.</item>
/// <item>Includes <see cref="SigningCredentials"/> for JWT issuance.</item>
/// <item>Contains associated <see cref="KeyMetadata"/> (creation time, status, etc.).</item>
/// <item>Exposes raw RSA parameters <c>N</c> (modulus) and <c>E</c> (exponent) for JWKS export.</item>
/// </list>
/// </remarks>
public record KeyEntry(
    RsaSecurityKey Key,
    SigningCredentials Signing,
    KeyMetadata Meta,
    string N,
    string E
);
