using Application.Features.KeyManagement.DTO;
using Microsoft.IdentityModel.Tokens;

namespace Application.Features.KeyManagement.Interfaces;

/// <summary>
/// Defines a contract for generating RSA keys used in JWT signing and encryption.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Creates new <see cref="RsaSecurityKey"/> instances with related metadata for identification and management.</item>
/// <item>Supports use in key rotation, initialization, or cryptographic provisioning workflows.</item>
/// <item>Allows configurable RSA key size (default 4096 bits) for enhanced security flexibility.</item>
/// </list>
/// </remarks>
public interface IKeyGenerator
{
    /// <summary>
    /// Generates a new RSA key pair along with its associated metadata.
    /// </summary>
    /// <param name="rsaBits">The RSA key size in bits (default is 4096).</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><see cref="RsaSecurityKey"/> representing the generated RSA key.</item>
    /// <item><see cref="KeyMetadata"/> containing metadata such as key identifier and creation time.</item>
    /// </list>
    /// </returns>
    (RsaSecurityKey Key, KeyMetadata Meta) Generate(int rsaBits = 4096);
}