using System.Text.Json.Serialization;

namespace Application.DTO.Key;

/// <summary>
/// Represents a public JSON Web Key (JWK) exposed in the JWKS endpoint.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Conforms to RFC 7517 (JSON Web Key) and RFC 7518 (JWA).</item>
/// <item>Used for JWT signature verification (typically RSA public keys).</item>
/// <item>Returned by the <c>.well-known/jwks.json</c> endpoint for public key discovery.</item>
/// </list>
/// </remarks>
public record PublicJwkDto(
    [property: JsonPropertyName("kty")] string Kty,
    [property: JsonPropertyName("use")] string Use,
    [property: JsonPropertyName("kid")] string Kid,
    [property: JsonPropertyName("alg")] string Alg,
    [property: JsonPropertyName("n")] string N,
    [property: JsonPropertyName("e")] string E,
    [property: JsonPropertyName("x5c")] string[] X5c
) {
    /// <summary>
    /// Initializes a new <see cref="PublicJwkDto"/> with standard defaults (RSA/RS256).
    /// </summary>
    public PublicJwkDto() 
        : this("RSA", "sig", string.Empty, "RS256", string.Empty, string.Empty, []) {}
}