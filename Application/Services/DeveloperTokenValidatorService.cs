using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Application.Interfaces;
using Application.Interfaces.JWTKey;
using Domain.Entities;

namespace Application.Services;

/// <summary>
/// Validates <see cref="DeveloperToken"/> JWTs using the active JWT keystore and token-key bindings.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Reads the JWT token and extracts KID and JTI.</item>
/// <item>Checks that a binding exists for the token ID and signing key.</item>
/// <item>Validates the token signature and lifetime.</item>
/// <item>Builds a <see cref="DeveloperTokenPrincipal"/> with developer ID, name, and scopes.</item>
/// </list>
/// </remarks>
public class DeveloperTokenValidatorService(
    IJwtKeyStore jwtKeyStore,
    IKeyBindingService keyBinding,
    ILogger<DeveloperTokenValidatorService> logger
) : IDeveloperTokenValidator
{
    private readonly JwtSecurityTokenHandler _handler = new();
    
    /// <inheritdoc />
    public async Task<DeveloperTokenPrincipal?> ValidateAsync(string jwtToken)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
            return null;

        var jwt = ReadJwtToken(jwtToken);
        if (jwt == null)
            return null;

        if (!TryGetTokenId(jwt, out var tokenId))
        {
            logger.LogWarning("[Validator] Missing or invalid jti");
            return null;
        }

        var kid = jwt.Header.Kid;
        if (string.IsNullOrEmpty(kid))
        {
            logger.LogWarning("[Validator] Missing kid in token header");
            return null;
        }

        if (!await IsBindingValid(tokenId, kid))
            return null;

        var signingCreds = jwtKeyStore.GetSigningCredentialsByKid(kid);
        if (signingCreds == null)
        {
            logger.LogWarning("[Validator] No signing creds for kid={Kid}", kid);
            return null;
        }

        return ValidateSignatureAndBuildPrincipal(jwtToken, signingCreds);
    }

    private JwtSecurityToken? ReadJwtToken(string token)
    {
        try { return _handler.ReadJwtToken(token); }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Validator] Token read failed");
            return null;
        }
    }

    private static bool TryGetTokenId(JwtSecurityToken jwt, out Guid tokenId)
    {
        var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        return Guid.TryParse(jti, out tokenId);
    }

    private async Task<bool> IsBindingValid(Guid tokenId, string kid)
    {
        var binding = await keyBinding.GetBindingAsync(tokenId, kid);
        if (binding == null)
        {
            logger.LogWarning("[Validator] No binding found for TokenId={TokenId}, Kid={Kid}", tokenId, kid);
            return false;
        }
        return true;
    }

    private DeveloperTokenPrincipal? ValidateSignatureAndBuildPrincipal(string jwtToken, SigningCredentials creds)
    {
        try
        {
            var principal = _handler.ValidateToken(jwtToken, new TokenValidationParameters
            {
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = creds.Key
            }, out _);

            return new DeveloperTokenPrincipal(
                GetClaimValue<Guid>(principal, "sub"),
                GetClaimValue<string>(principal, "name"),
                principal.Claims.Where(c => c.Type == "scope").Select(c => c.Value)
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Validator] Signature/lifetime validation failed");
            return null;
        }
    }

    private static T GetClaimValue<T>(System.Security.Claims.ClaimsPrincipal principal, string type)
    {
        var value = principal.Claims.First(c => c.Type == type).Value;
        return (T)Convert.ChangeType(value, typeof(T));
    }
}
