using Core.KeyManagement.DTO;
using Microsoft.IdentityModel.Tokens;

namespace Core.KeyManagement.Interfaces;

/// <summary>
/// Defines contract for generating RSA key pairs used by the JWT key management system.
/// </summary>
/// <remarks>
/// <para>
/// generate RSA key pairs together with the metadata required
/// to identify and manage the generated keys throughout their lifecycle.
/// keys can be used during initial key store provisioning, key
/// rotation, or other cryptographic key management operations.
/// The RSA key size can be configured by the caller. Implementations should
/// enforce any minimum or maximum key size required by their security policy.
/// </para>
/// </remarks>
public interface IKeyGenerator
{
    /// <summary>
    /// Generates new RSA key pair and its associated metadata.
    /// </summary>
    /// <param name="rsaBits">The RSA key size in bits. Defaults to <c>4096</c>.</param>
    /// <returns>
    /// A tuple containing the generated <see cref="RsaSecurityKey"/> and
    /// its associated <see cref="KeyMetadata"/>.
    /// </returns>
    /// <remarks>
    /// The generated key should contain both public and private key material
    /// and must be protected appropriately when persisted.
    /// </remarks>
    (RsaSecurityKey Key, KeyMetadata Meta) Generate(int rsaBits = 4096);
}
