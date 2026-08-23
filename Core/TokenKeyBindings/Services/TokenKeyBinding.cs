namespace Core.TokenKeyBindings.Services;

/// <summary>
/// Represents binding between developer token and the RSA signing key
/// used to sign its JWTs.
/// </summary>
/// <remarks>
/// <para>
/// Each binding associates a developer token with a specific signing key
/// identified by <see cref="SigningKeyId"/>.
/// The binding stores the corresponding public key used for signature
/// verification and supports key rotation, public key updates, and revocation.
/// </para>
/// </remarks>
public sealed record TokenKeyBinding
{
    /// <summary>
    /// Gets the unique identifier of the developer token associated with this binding.
    /// </summary>
    public Guid TokenId { get; init; }

    /// <summary>
    /// Gets the unique identifier of the RSA signing key associated with this binding.
    /// </summary>
    public string SigningKeyId { get; private set; } = null!;

    /// <summary>
    /// Gets the public key associated with the signing key.
    /// </summary>
    public string PublicKey { get; private set; } = null!;

    /// <summary>
    /// Gets the date and time at which the current key binding was established
    /// or last changed.
    /// </summary>
    public DateTimeOffset BoundAt { get; private set; }

    /// <summary>
    /// Gets value indicating whether this key binding has been revoked.
    /// </summary>
    public bool Revoked { get; private set; }

    /// <summary>
    /// Initializes new instance of the <see cref="TokenKeyBinding"/> record.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token. </param>
    /// <param name="signingKeyId">The unique identifier of the RSA signing key. </param>
    /// <param name="publicKey">The public key associated with the signing key.  </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="signingKeyId"/> or
    /// <paramref name="publicKey"/> is <c>null</c>.
    /// </exception>
    public TokenKeyBinding(
        Guid tokenId,
        string signingKeyId,
        string publicKey)
    {
        TokenId = tokenId;
        SigningKeyId = signingKeyId
            ?? throw new ArgumentNullException(nameof(signingKeyId));
        PublicKey = publicKey
            ?? throw new ArgumentNullException(nameof(publicKey));
        BoundAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Creates a new binding state associated with a different RSA signing key.
    /// </summary>
    /// <param name="newSigningKeyId">The unique identifier of the new signing key. </param>
    /// <param name="newPublicKey">The public key associated with the new signing key. </param>
    /// <returns><see cref="TokenKeyBinding"/> containing the new signing key and public key information. </returns>
    public TokenKeyBinding Rebind(
        string newSigningKeyId,
        string newPublicKey)
        => this with
        {
            SigningKeyId = newSigningKeyId,
            PublicKey = newPublicKey,
            BoundAt = DateTimeOffset.UtcNow
        };

    /// <summary>
    /// Creates a new binding state with an updated public key.
    /// </summary>
    /// <param name="updatedPublicKey">The new public key associated with the current signing key. </param>
    /// <returns>A new <see cref="TokenKeyBinding"/> containing the updated public key.</returns>
    public TokenKeyBinding UpdatePublicKey(string updatedPublicKey)
        => this with
        {
            PublicKey = updatedPublicKey,
            BoundAt = DateTimeOffset.UtcNow
        };

    /// <summary>
    /// Creates a new binding state with the binding marked as revoked.
    /// </summary>
    /// <returns>A new <see cref="TokenKeyBinding"></see>
    ///     with <see cref="Revoked"/> set to <c>true</c>. </returns>
    public TokenKeyBinding Revoke()
        => this with
        {
            Revoked = true,
            BoundAt = DateTimeOffset.UtcNow
        };
}