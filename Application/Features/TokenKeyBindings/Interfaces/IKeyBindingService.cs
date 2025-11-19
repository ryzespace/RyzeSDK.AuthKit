using Domain.Features.DeveloperTokens;
using Domain.Features.TokenKeyBindings;

namespace Application.Features.TokenKeyBindings.Interfaces;

/// <summary>
/// Abstraction for managing key bindings between <see cref="DeveloperToken"/> and RSA signing keys.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Creates, updates, rebinds, and revokes token-to-key bindings.</item>
/// <item>Provides retrieval and listing of key bindings by token or signing key.</item>
/// <item>Designed for asynchronous operations in a DDD-compliant context.</item>
/// </list>
/// </remarks>
public interface IKeyBindingService
{
    /// <summary>
    /// Creates a new key binding for a given developer token.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <param name="signingKeyId">The ID of the signing key to bind.</param>
    /// <param name="publicKey">The public key associated with the signing key.</param>
    /// <returns>The newly created <see cref="TokenKeyBinding"/>.</returns>
    Task<TokenKeyBinding> CreateBindingAsync(Guid tokenId, string signingKeyId, string publicKey);

    /// <summary>
    /// Rebinds an existing key binding to a new signing key.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <param name="signingKeyId">The current signing key ID to rebind from.</param>
    /// <param name="newSigningKeyId">The new signing key ID to bind to.</param>
    /// <param name="newPublicKey">The public key of the new signing key.</param>
    /// <returns>The updated <see cref="TokenKeyBinding"/>, or <c>null</c> if the original binding does not exist.</returns>
    Task<TokenKeyBinding?> RebindAsync(Guid tokenId, string signingKeyId, string newSigningKeyId, string newPublicKey);

    /// <summary>
    /// Updates the public key for an existing key binding without changing the signing key ID.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <param name="signingKeyId">The signing key ID whose public key is being updated.</param>
    /// <param name="newPublicKey">The new public key value.</param>
    /// <returns>The updated <see cref="TokenKeyBinding"/>, or <c>null</c> if no binding exists.</returns>
    Task<TokenKeyBinding?> UpdatePublicKeyAsync(Guid tokenId, string signingKeyId, string newPublicKey);

    /// <summary>
    /// Revokes a key binding by marking it as inactive or revoked.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <returns><c>true</c> if revocation succeeded; otherwise <c>false</c>.</returns>
    Task<bool> RevokeAsync(string tokenId);

    /// <summary>
    /// Lists all key bindings associated with a given developer token.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <returns>A collection of <see cref="TokenKeyBinding"/> entities.</returns>
    Task<IEnumerable<TokenKeyBinding>> ListBindingsAsync(Guid tokenId);

    /// <summary>
    /// Gets a specific key binding by token ID and signing key ID.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <param name="signingKeyId">The signing key ID to retrieve.</param>
    /// <returns>The <see cref="TokenKeyBinding"/> if found; otherwise <c>null</c>.</returns>
    Task<TokenKeyBinding?> GetBindingAsync(Guid tokenId, string signingKeyId);
}
