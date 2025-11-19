using System.Security.Cryptography;
using Application.Features.KeyManagement.DTO;
using Application.Features.KeyManagement.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Application.Features.KeyManagement.Services;

/// <summary>
/// Generates new RSA key pairs with unique identifiers and creation metadata.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Uses 4096-bit RSA by default for strong security.</item>
/// <item>Assigns a random GUID-based Key ID (KID) to each key.</item>
/// <item>Returns both the key and its associated metadata for persistence or rotation.</item>
/// </list>
/// </remarks>
public class RsaKeyGenerator : IKeyGenerator
{
    /// <inheritdoc />
    public (RsaSecurityKey Key, KeyMetadata Meta) Generate(int rsaBits = 4096)
    {
        var rsa = RSA.Create(rsaBits);
        var kid = Guid.NewGuid().ToString("N");

        var key = new RsaSecurityKey(rsa)
      //  var key = new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: true))
        {
            KeyId = kid
        };

        var meta = new KeyMetadata(kid, DateTime.UtcNow, false);
        return (key, meta);
    }
}