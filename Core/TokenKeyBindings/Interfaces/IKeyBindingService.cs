using Core.TokenKeyBindings.Services;

namespace Core.TokenKeyBindings.Interfaces;

/// <summary>
/// Defines contract for managing bindings between developer tokens and RSA signing keys.
/// </summary>
/// <remarks>
/// <para>
/// Provides operations for creating, updating, rebinding, retrieving, listing,
/// and revoking token to key bindings.
///
/// Key bindings associate developer token with specific signing key and
/// its corresponding public key material.
/// </para>
/// </remarks>
public interface IKeyBindingService
{
    /// <summary>
    /// Creates new key binding for developer token.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <param name="signingKeyId">The unique identifier of the signing key to bind.</param>
    /// <param name="publicKey">The public key associated with the signing key.</param>
    /// <returns>The newly created <see cref="TokenKeyBinding"/>.</returns>
    Task<TokenKeyBinding> CreateBindingAsync(
        Guid tokenId,
        string signingKeyId,
        string publicKey);

    /// <summary>
    /// Rebinds an existing key binding to different signing key.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <param name="signingKeyId">The identifier of the signing key currently associated with the binding.</param>
    /// <param name="newSigningKeyId">The identifier of the new signing key.</param>
    /// <param name="newPublicKey">The public key associated with the new signing key.</param>
    /// <returns>
    /// The updated <see cref="TokenKeyBinding"/>, or <c>null</c> if the
    /// existing binding could not be found.
    /// </returns>
    Task<TokenKeyBinding?> RebindAsync(
        Guid tokenId,
        string signingKeyId,
        string newSigningKeyId,
        string newPublicKey);

    /// <summary>
    /// Updates the public key associated with an existing key binding.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <param name="signingKeyId">The identifier of the signing key whose public key is being updated.</param>
    /// <param name="newPublicKey">The new public key associated with the signing key.</param>
    /// <returns>
    /// The updated <see cref="TokenKeyBinding"/>, or <c>null</c> if the
    /// binding could not be found.
    /// </returns>
    Task<TokenKeyBinding?> UpdatePublicKeyAsync(
        Guid tokenId,
        string signingKeyId,
        string newPublicKey);

    /// <summary>
    /// Revokes all key bindings associated with developer token.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <returns>
    /// <c>true</c> if one or more bindings were successfully revoked;
    /// otherwise, <c>false</c>.
    /// </returns>
    Task<bool> RevokeAsync(Guid tokenId);

    /// <summary>
    /// Lists all key bindings associated with a developer token.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token. </param>
    /// <returns>
    /// A collection of <see cref="TokenKeyBinding"/> entities associated
    /// with the specified developer token.
    /// </returns>
    Task<IEnumerable<TokenKeyBinding>> ListBindingsAsync(Guid tokenId);

    /// <summary>
    /// Retrieves specific key binding for developer token and signing key.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <param name="signingKeyId">The identifier of the signing key to retrieve.</param>
    /// <returns>
    /// The matching <see cref="TokenKeyBinding"/>, or <c>null</c> if no
    /// matching binding exists.
    /// </returns>
    Task<TokenKeyBinding?> GetBindingAsync(
        Guid tokenId,
        string signingKeyId);
}
