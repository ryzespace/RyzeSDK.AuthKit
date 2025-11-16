namespace Domain.Entities;

/// <summary>
/// Represents a binding between a <see cref="DeveloperToken"/> and the RSA signing key used to sign its JWT.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Each binding associates a developer token with a specific signing key (identified by <see cref="SigningKeyId"/>).</item>
/// <item>Contains the public key used for verification in JWKS endpoints.</item>
/// <item>Supports re-binding in case of key rotation or revocation.</item>
/// </list>
/// </remarks>
public record TokenKeyBinding
{
    public Guid TokenId { get; init; }
    public string SigningKeyId { get; private set; } = null!;
    public string PublicKey { get; private set; } = null!;
    public DateTimeOffset BoundAt { get; private set; }
    public bool Revoked { get; private set; }
    
    public TokenKeyBinding(Guid tokenId, string signingKeyId, string publicKey)
    {
        TokenId = tokenId;
        SigningKeyId = signingKeyId ?? throw new ArgumentNullException(nameof(signingKeyId));
        PublicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        BoundAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Rebinds this token to a new RSA signing key.
    /// </summary>
    public TokenKeyBinding Rebind(string newSigningKeyId, string newPublicKey)
        => this with { SigningKeyId = newSigningKeyId, PublicKey = newPublicKey, BoundAt = DateTimeOffset.UtcNow };
    
    /// <summary>
    /// Updates the public key without changing the key ID.
    /// </summary>
    public TokenKeyBinding UpdatePublicKey(string updatedPublicKey)
        => this with { PublicKey = updatedPublicKey, BoundAt = DateTimeOffset.UtcNow };
    
    public TokenKeyBinding Revoke()
        => this with { Revoked = true, BoundAt = DateTimeOffset.UtcNow };
}
