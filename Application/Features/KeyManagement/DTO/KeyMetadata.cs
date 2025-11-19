namespace Application.Features.KeyManagement.DTO;

/// <summary>
/// Represents metadata describing a cryptographic signing key.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Contains key identifier (<c>kid</c>), creation date, algorithm, and revocation status.</item>
/// <item>Used to manage rotation and public JWTS publication.</item>
/// </list>
/// </remarks>
public sealed record KeyMetadata(
    string Kid,
    DateTimeOffset CreatedAt,
    bool Revoked,
    string Algorithm = "RS256",
    string Purpose = "JWT signing"
);