using System.Text.Json.Serialization;
using Application.Features.KeyManagement.DTO;

namespace Infrastructure.Restful.Jwks.DTO.Response;

/// <summary>
/// Represents a JSON Web Key Set (JWKS) response containing all active public keys.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Compliant with RFC 7517 (JSON Web Key Set).</item>
/// <item>Contains only non-revoked, public RSA keys used for signature verification.</item>
/// </list>
/// </remarks>
public record JwksResponse(
    [property: JsonPropertyName("keys")] IEnumerable<PublicJwkDto> Keys
);