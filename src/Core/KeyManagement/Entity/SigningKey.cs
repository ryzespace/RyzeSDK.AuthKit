namespace Core.KeyManagement.Entity;

/// <summary>
/// Represents cryptographic signing key managed by the key management system.
/// </summary>
/// <remarks>
/// <para>
/// A signing key contains both its public representation and protected private
/// key material together with lifecycle information used to determine whether
/// the key may be used to sign new tokens.
/// </para>
/// <para>
/// The key lifecycle is represented by its activation, validity, expiration,
/// and revocation state. State transitions are performed through the methods
/// exposed by this entity.
/// </para>
/// </remarks>
public sealed record SigningKey
{
    /// <summary>
    /// Gets the unique identifier of the signing key.
    /// </summary>
    /// <remarks>
    /// This identifier is used as the JWT <c>kid</c> value to associate a
    /// token with the public key required to verify its signature.
    /// </remarks>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the public RSA key encoded as PEM.
    /// </summary>
    /// <remarks>
    /// This value contains public key material and may be exposed to
    /// consumers through the appropriate public key representation.
    /// </remarks>
    public required string PublicKeyPem { get; init; }

    /// <summary>
    /// Gets the encrypted private key material encoded for persistence.
    /// </summary>
    /// <remarks>
    /// The value contains sensitive cryptographic material and must be
    /// protected from unauthorized access.
    /// </remarks>
    public required string PrivateKeyEncrypted { get; init; }

    /// <summary>
    /// Gets the cryptographic algorithm used when signing tokens.
    /// </summary>
    /// <remarks>For RSA signing keys this will typically be an algorithm such as RS256. </remarks>
    public required string Algorithm { get; init; }

    /// <summary>
    /// Gets the date and time at which the signing key was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the date and time before which the key must not be used for signing.
    /// </summary>
    /// <value>
    /// <c>null</c> when the key has no activation restriction.
    /// </value>
    public DateTime? NotBefore { get; init; }

    /// <summary>
    /// Gets the date and time after which the key must no longer be used for signing.
    /// </summary>
    /// <value>
    /// <c>null</c> when the key does not have an expiration time.
    /// </value>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// Gets the date and time at which the key was revoked.
    /// </summary>
    /// <value>
    /// <c>null</c> when the key has not been revoked.
    /// </value>
    public DateTime? RevokedAt { get; init; }

    /// <summary>
    /// Gets value indicating whether the key is currently active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Determines whether the signing key is currently valid for signing tokens.
    /// </summary>
    /// <param name="now">
    /// The current date and time used to evaluate the key lifecycle.
    /// </param>
    /// <returns>
    /// <c>true</c> when the key is active, has not been revoked, is past its
    /// activation time, and has not expired; otherwise, <c>false</c>.
    /// </returns>
    public bool IsValidForSigning(DateTime now)
        => IsActive &&
           RevokedAt is null &&
           (NotBefore is null || now >= NotBefore) &&
           (ExpiresAt is null || now < ExpiresAt);

    /// <summary>
    /// Activates the signing key.
    /// </summary>
    /// <param name="now">The date and time at which the key becomes active.</param>
    /// <returns>A new <see cref="SigningKey"/> instance representing the activated key. </returns>
    /// <remarks>
    /// Activating key sets its <see cref="NotBefore"/> timestamp to
    /// <paramref name="now"/> and clears any previous revocation timestamp.
    /// </remarks>
    public SigningKey Activate(DateTime now)
        => this with
        {
            IsActive = true,
            NotBefore = now,
            RevokedAt = null
        };

    /// <summary>
    /// Revokes the signing key.
    /// </summary>
    /// <param name="now">The date and time at which the key is revoked.</param>
    /// <returns>A new <see cref="SigningKey"/> instance representing the revoked key.</returns>
    /// <remarks>
    /// A revoked key is immediately marked as inactive and cannot be used
    /// for signing new tokens.
    /// </remarks>
    public SigningKey Revoke(DateTime now)
        => this with
        {
            IsActive = false,
            RevokedAt = now
        };

    /// <summary>
    /// Marks the signing key as expired.
    /// </summary>
    /// <param name="now">The date and time at which the key expires.</param>
    /// <returns>A new <see cref="SigningKey"/> instance representing the expired key.</returns>
    /// <remarks>
    /// Expiring key marks it as inactive and sets its expiration timestamp
    /// to <paramref name="now"/>.
    /// </remarks>
    public SigningKey Expire(DateTime now)
        => this with
        {
            IsActive = false,
            ExpiresAt = now
        };
}
