using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Application.Features.DeveloperTokens.DTO;
using Application.Features.DeveloperTokens.Interfaces;
using Application.Features.KeyManagement.Interfaces;
using Application.Features.TokenKeyBindings.Interfaces;
using Domain.Features.DeveloperTokens;

namespace Application.Features.DeveloperTokens.Services;

/// <summary>
/// Service responsible for generating and managing <see cref="DeveloperToken"/> JWTs.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Generates JWTs for developer tokens using the active key from <see cref="IJwtKeyStore"/>.</item>
/// <item>Creates key bindings between tokens and RSA signing keys via <see cref="IKeyBindingService"/>.</item>
/// <item>Supports short-lived ("temp") and permanent ("live") token formats.</item>
/// <item>Encapsulates JWT claim construction and secure random short key generation.</item>
/// </list>
/// </remarks>
public class DeveloperTokenService(IJwtKeyStore jwtKeyStore, IKeyBindingService keyBindingService)
    : IDeveloperTokenService
{
    /// <inheritdoc />
    public async Task<DeveloperTokenPairDto> GenerateToken(DeveloperToken token)
    {
        var creds = GetSigningCredentials();
        var claims = BuildClaims(token);
        var jwt = CreateJwtToken(claims, token.Lifetime.ExpiresAt, creds);

        await CreateKeyBinding(token.Id, creds.Key.KeyId!);

        var handler = new JwtSecurityTokenHandler();
        return new DeveloperTokenPairDto(GenerateShortKey(token), handler.WriteToken(jwt));
    }

    private Microsoft.IdentityModel.Tokens.SigningCredentials GetSigningCredentials()
    {
        var creds = jwtKeyStore.GetActiveSigningCredentials();
        return string.IsNullOrEmpty(creds.Key.KeyId)
            ? throw new InvalidOperationException("SigningCredentials must have a KeyId set.") : creds;
    }

    private async Task CreateKeyBinding(Guid tokenId, string signingKeyId)
    {
        var jwk = jwtKeyStore.GetPublicJwks()
            .FirstOrDefault(k => k.Kid == signingKeyId)
            ?? throw new InvalidOperationException($"No JWK for kid '{signingKeyId}'.");

        await keyBindingService.CreateBindingAsync(tokenId, signingKeyId, jwk.N);
    }

    private static JwtSecurityToken CreateJwtToken(IEnumerable<Claim> claims, DateTimeOffset? expiresAt,
        Microsoft.IdentityModel.Tokens.SigningCredentials creds)
    {
        var expires = expiresAt?.UtcDateTime ?? DateTime.UtcNow.AddYears(100);
        return new JwtSecurityToken(claims: claims, expires: expires, signingCredentials: creds);
    }

    private static IEnumerable<Claim> BuildClaims(DeveloperToken token)
    {
        yield return new Claim(JwtRegisteredClaimNames.Sub, token.DeveloperId.ToString());
        yield return new Claim(JwtRegisteredClaimNames.Jti, token.Id.ToString());
        yield return new Claim("name", token.Name);
        yield return new Claim("type", token.Lifetime.ExpiresAt.HasValue ? "temp" : "live");

        foreach (var scope in token.Scopes)
            yield return new Claim("scope", scope.Value);
    }

    private static string GenerateShortKey(DeveloperToken token)
    {
        var prefix = token.Lifetime.ExpiresAt.HasValue ? "rk_temp_" : "rk_live_";
        var randomHex = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        return $"{prefix}{randomHex}";
    }
}
