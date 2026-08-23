using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Core.KeyManagement.Interfaces;
using Core.TokenKeyBindings.Interfaces;
using DevTokens.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace DevTokens.Services;

/// <summary>
/// Validates developer token JWTs using the configured JWT key store
/// and token to signing key bindings.
/// </summary>
/// <remarks>
/// <para>
/// Extracts the token identifier (<c>jti</c>) and signing key identifier
/// (<c>kid</c>) from the JWT and verifies that corresponding key binding exists.
/// </para>
/// <para>
/// Validates the JWT signature and lifetime using the signing credentials
/// associated with the specified key identifier.
/// </para>
/// <para>
/// When validation succeeds, creates <see cref="DeveloperTokenPrincipal"/>
/// containing the developer identifier, token name, and assigned scopes.
/// </para>
/// </remarks>
public class DeveloperTokenValidatorService(
    IJwtKeyStore jwtKeyStore,
    IKeyBindingService keyBinding,
    ILogger<DeveloperTokenValidatorService> logger
) : IDeveloperTokenValidator
{
    private readonly JwtSecurityTokenHandler _handler = new();

    /// <summary>
    /// Validates developer token JWT and builds its authenticated principal.
    /// </summary>
    /// <param name="jwtToken">The JWT to validate.</param>
    /// <returns>
    /// A <see cref="DeveloperTokenPrincipal"/> when the token is valid;
    /// otherwise, <c>null</c>.
    /// </returns>
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
        if (signingCreds != null)
            return ValidateSignatureAndBuildPrincipal(jwtToken, signingCreds);
       
        logger.LogWarning("[Validator] No signing creds for kid={Kid}", kid);
        return null;

    }

    /// <summary>
    /// Attempts to parse the supplied string as JWT.
    /// </summary>
    /// <param name="token">The JWT string to parse.</param>
    /// <returns>
    /// The parsed <see cref="JwtSecurityToken"/>, or <c>null</c> if parsing fails.
    /// </returns>
    private JwtSecurityToken? ReadJwtToken(string token)
    {
        try
        {
            return _handler.ReadJwtToken(token);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Validator] Token read failed");
            return null;
        }
    }

    /// <summary>
    /// Attempts to extract the developer token identifier from the JWT <c>jti</c> claim.
    /// </summary>
    /// <param name="jwt">The parsed JWT.</param>
    /// <param name="tokenId">The parsed token identifier.</param>
    /// <returns>
    /// <c>true</c> when the <c>jti</c> claim contains a valid <see cref="Guid"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    private static bool TryGetTokenId(
        JwtSecurityToken jwt,
        out Guid tokenId)
    {
        var jti = jwt.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)
            ?.Value;

        return Guid.TryParse(jti, out tokenId);
    }

    /// <summary>
    /// Verifies that a token-to-signing-key binding exists.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <param name="kid">The signing key identifier from the JWT header.</param>
    /// <returns>
    /// <c>true</c> when a matching binding exists; otherwise, <c>false</c>.
    /// </returns>
    private async Task<bool> IsBindingValid(
        Guid tokenId,
        string kid)
    {
        var binding = await keyBinding.GetBindingAsync(tokenId, kid);

        if (binding != null) return true;
        logger.LogWarning("[Validator] No binding found for TokenId={TokenId}, Kid={Kid}", tokenId, kid);
        return false;

    }

    /// <summary>
    /// Validates the JWT signature and lifetime and creates the corresponding principal.
    /// </summary>
    /// <param name="jwtToken">The JWT to validate.</param>
    /// <param name="creds">The signing credentials associated with the token's signing key.</param>
    /// <returns>
    /// A <see cref="DeveloperTokenPrincipal"/> when validation succeeds;
    /// otherwise, <c>null</c>.
    /// </returns>
    private DeveloperTokenPrincipal? ValidateSignatureAndBuildPrincipal(
        string jwtToken,
        SigningCredentials creds)
    {
        try
        {
            var principal = _handler.ValidateToken(
                jwtToken,
                new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = creds.Key
                },
                out _);

            return new DeveloperTokenPrincipal(
                GetClaimValue<Guid>(principal, "sub"),
                GetClaimValue<string>(principal, "name"),
                principal.Claims
                    .Where(c => c.Type == "scope")
                    .Select(c => c.Value)
            );
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[Validator] Signature/lifetime validation failed");
            return null;
        }
    }

    /// <summary>
    /// Retrieves and converts a claim value from the specified principal.
    /// </summary>
    /// <typeparam name="T">The expected claim value type.</typeparam>
    /// <param name="principal">The claims principal containing the claim.</param>
    /// <param name="type">The claim type to retrieve.</param>
    /// <returns>The converted claim value.</returns>
    private static T GetClaimValue<T>(
        ClaimsPrincipal principal,
        string type)
    {
        var value = principal.Claims.First(c => c.Type == type).Value;
        return (T)Convert.ChangeType(value, typeof(T));
    }
}