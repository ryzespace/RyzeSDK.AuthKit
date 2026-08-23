using System.Security.Cryptography;
using Core.KeyManagement.DTO;
using Core.KeyManagement.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Core.KeyManagement.Services;

/// <summary>
/// Generates RSA key pairs with unique identifiers and associated metadata.
/// </summary>
/// <remarks>
/// <para>
/// Uses RSA keys for JWT signing and generates a unique key identifier (KID)
/// for every newly created key.
///
/// The generated <see cref="RsaSecurityKey"/> retains the RSA private key
/// material and can therefore be used for signing and secure persistence.
/// The default key size is 4096 bits and can be configured when generating a new key.
/// </para>
/// </remarks>
public sealed class RsaKeyGenerator : IKeyGenerator
{
    /// <summary>
    /// Generates new RSA key pair and its associated metadata.
    /// </summary>
    /// <param name="rsaBits">The RSA key size in bits. Defaults to 4096 bits. </param>
    /// <returns>
    /// A tuple containing the generated <see cref="RsaSecurityKey"/> and
    /// its associated <see cref="KeyMetadata"/>.
    /// </returns>
    public (RsaSecurityKey Key, KeyMetadata Meta) Generate(int rsaBits = 4096)
    {
        var rsa = RSA.Create(rsaBits);
        var kid = Guid.NewGuid().ToString("N");

        var key = new RsaSecurityKey(rsa)
        { 
            KeyId = kid
        };
        
        var meta = new KeyMetadata
        {
            Kid = kid,
            CreatedAt = DateTimeOffset.UtcNow,
            Revoked = false
        };

        return (key, meta);
    }
}
