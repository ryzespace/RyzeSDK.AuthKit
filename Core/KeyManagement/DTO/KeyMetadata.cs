namespace Core.KeyManagement.DTO;

/// <summary>
/// Represents metadata describing cryptographic signing key.
/// </summary>
/// <remarks>
/// <para>
/// The metadata identifies signing key and describes its lifecycle,
/// cryptographic algorithm, and intended purpose.
/// </para>
/// <para>
/// This information is used to manage key rotation, revocation, and
/// publication of public key metadata through JWKS.
/// </para>
/// </remarks>
public sealed record KeyMetadata
{
    /// <summary>
    /// Gets the unique key identifier (kid).
    /// </summary>
    /// <remarks>
    /// The identifier is used to associate JWT with the public key
    /// required to validate its signature.
    /// </remarks>
    public required string Kid { get; init; }

    /// <summary>
    /// Gets the date and time at which the key was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets value indicating whether the key has been revoked.
    /// </summary>
    /// <remarks>
    /// A revoked key should no longer be used for signing new tokens.
    /// Depending on the key management policy, its public material may
    /// remain available for validating tokens issued before revocation.
    /// </remarks>
    public bool Revoked { get; init; }

    /// <summary>
    /// Gets the cryptographic algorithm used by the key.
    /// </summary>
    /// <remarks> Defaults to RS256.</remarks>
    public string Algorithm { get; init; } = "RS256";

    /// <summary>
    /// Gets the intended purpose of the key.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>JWT signing</c>.
    /// </remarks>
    public string Purpose { get; init; } = "JWT signing";
}