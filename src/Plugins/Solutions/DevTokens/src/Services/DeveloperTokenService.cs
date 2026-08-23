using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Core.KeyManagement.Interfaces;
using Core.TokenKeyBindings.Interfaces;
using DevTokens.DTO;
using DevTokens.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace DevTokens.Services;

/// <summary>
/// Service responsible for generating JWT credentials for developer tokens.
/// </summary>
/// <remarks>
/// <para>
/// Uses the active signing key from <see cref="IJwtKeyStore"/> to create
/// signed JSON Web Tokens for developer tokens.
/// </para>
/// <para>
/// Creates signing key binding between the developer token and the RSA key
/// used to sign its JWT through <see cref="IKeyBindingService"/>.
/// </para>
/// <para>
/// Generates both the signed JWT and short API key identifier. Tokens are
/// distinguished as temporary or permanent based on whether an expiration
/// date is configured.
/// </para>
/// </remarks>
public class DeveloperTokenService(
    IJwtKeyStore jwtKeyStore,
    IKeyBindingService keyBindingService) : IDeveloperTokenService
{
    /// <summary>
    /// Generates JWT credentials for the specified developer token.
    /// </summary>
    /// <param name="token">The developer token for which credentials are generated.</param>
    /// <returns>A <see cref="DeveloperTokenPairDto"/> containing the generated short API key and signed JWT. </returns>
    public async Task<DeveloperTokenPairDto> GenerateToken(DeveloperToken token)
    {
        var creds = GetSigningCredentials();
        var claims = BuildClaims(token);
       
        var jwt = CreateJwtToken(claims, token.Lifetime.ExpiresAt, creds);
        await CreateKeyBinding(token.Id, creds.Key.KeyId!);

        var handler = new JwtSecurityTokenHandler();
        return new DeveloperTokenPairDto(
            GenerateShortKey(token),
            handler.WriteToken(jwt));
    }

    /// <summary>
    /// Retrieves the active JWT signing credentials.
    /// </summary>
    /// <returns>The active signing credentials with a valid key identifier.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the active signing key does not have a key identifier.</exception>
    private SigningCredentials GetSigningCredentials()
    {
        var creds = jwtKeyStore.GetActiveSigningCredentials();
        return string.IsNullOrEmpty(creds.Key.KeyId)
            ? throw new InvalidOperationException(
                "SigningCredentials must have a KeyId set.")
            : creds;
    }

    /// <summary>
    /// Creates binding between the developer token and its signing key.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <param name="signingKeyId">The identifier of the signing key.</param>
    /// <returns>A task representing the asynchronous binding operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the public JWK corresponding to the signing key cannot be found. </exception>
    private async Task CreateKeyBinding(Guid tokenId, string signingKeyId)
    {
        var jwk = jwtKeyStore.GetPublicJwks()
            .FirstOrDefault(k => k.Kid == signingKeyId)
            ?? throw new InvalidOperationException(
                $"No JWK for kid '{signingKeyId}'.");

        await keyBindingService.CreateBindingAsync(
            tokenId,
            signingKeyId,
            jwk.N);
    }

    /// <summary>
    /// Creates a signed JWT for the specified claims and expiration time.
    /// </summary>
    /// <param name="claims">Claims to include in the JWT.</param>
    /// <param name="expiresAt">Optional expiration date of the token.</param>
    /// <param name="creds">Credentials used to sign the JWT.</param>
    /// <returns>A signed <see cref="JwtSecurityToken"/>.</returns>
    private static JwtSecurityToken CreateJwtToken(
        IEnumerable<Claim> claims,
        DateTimeOffset? expiresAt,
        SigningCredentials creds)
    {
        var expires = expiresAt?.UtcDateTime
            ?? DateTime.UtcNow.AddYears(100);

        return new JwtSecurityToken(
            claims: claims,
            expires: expires,
            signingCredentials: creds);
    }

    /// <summary>
    /// Builds the claims included in a developer token JWT.
    /// </summary>
    /// <param name="token">The developer token used as the source of the claims.</param>
    /// <returns>A sequence of JWT claims.</returns>
    private static IEnumerable<Claim> BuildClaims(DeveloperToken token)
    {
        yield return new Claim(JwtRegisteredClaimNames.Sub, token.DeveloperId.ToString());
        yield return new Claim(JwtRegisteredClaimNames.Jti, token.Id.ToString());
        yield return new Claim("name", token.Name);
        yield return new Claim("type",
            token.Lifetime.ExpiresAt.HasValue ? "temp" : "live");

        foreach (var scope in token.Scopes)
        {
            yield return new Claim(
                "scope",
                scope.Value);
        }
    }

    /// <summary>
    /// Generates a short API key for the specified developer token.
    /// </summary>
    /// <param name="token">The developer token for which the key is generated.</param>
    /// <returns>
    /// A randomly generated API key prefixed with <c>rk_temp_</c> for
    /// temporary tokens or <c>rk_live_</c> for permanent tokens.
    /// </returns>
    private static string GenerateShortKey(DeveloperToken token)
    {
        var prefix = token.Lifetime.ExpiresAt.HasValue
            ? "rk_temp_"
            : "rk_live_";

        var randomHex = Convert
            .ToHexString(RandomNumberGenerator.GetBytes(16))
            .ToLowerInvariant();

        return $"{prefix}{randomHex}";
    }
}