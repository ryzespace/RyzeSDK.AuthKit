using Core.TokenKeyBindings.Interfaces;

namespace Core.TokenKeyBindings.Services;

/// <summary>
/// Provides application level operations for managing bindings between
/// developer tokens and RSA signing keys.
/// </summary>
/// <remarks>
/// <para>
/// Coordinates key binding operations through <see cref="IKeyBindingRepository"/>
/// and applies the domain behavior exposed by <see cref="TokenKeyBinding"/>.
/// </para>
/// </remarks>
public sealed class KeyBindingService(IKeyBindingRepository repository) : IKeyBindingService
{
    /// <summary>
    /// Creates new key binding for the specified developer token.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token. </param>
    /// <param name="signingKeyId">The unique identifier of the RSA signing key to bind. </param>
    /// <param name="publicKey">The public key associated with the signing key. </param>
    /// <returns>The newly persisted <see cref="TokenKeyBinding"/>.</returns>
    public Task<TokenKeyBinding> CreateBindingAsync(
        Guid tokenId,
        string signingKeyId,
        string publicKey)
    {
        var binding = new TokenKeyBinding(
            tokenId,
            signingKeyId,
            publicKey);

        return repository.AddAsync(binding);
    }

    /// <summary>
    /// Rebinds an existing token key binding to a different signing key.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token. </param>
    /// <param name="signingKeyId">The identifier of the signing key currently associated with the binding. </param>
    /// <param name="newSigningKeyId">The identifier of the new signing key. </param>
    /// <param name="newPublicKey">The public key associated with the new signing key. </param>
    /// <returns>
    /// The updated <see cref="TokenKeyBinding"/>, or <c>null</c> if the
    /// existing binding could not be found.
    /// </returns>
    public async Task<TokenKeyBinding?> RebindAsync(
        Guid tokenId,
        string signingKeyId,
        string newSigningKeyId,
        string newPublicKey)
    {
        var binding = await repository.GetAsync(
            tokenId,
            signingKeyId);

        if (binding is null)
            return null;

        var updated = binding.Rebind(
            newSigningKeyId,
            newPublicKey);

        await repository.UpdateAsync(updated);

        return updated;
    }

    /// <summary>
    /// Updates the public key of an existing token key binding.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token. </param>
    /// <param name="signingKeyId">The identifier of the signing key associated with the binding. </param>
    /// <param name="newPublicKey">The new public key associated with the signing key. </param>
    /// <returns>
    /// The updated <see cref="TokenKeyBinding"/>, or <c>null</c> if the
    /// binding could not be found.
    /// </returns>
    public async Task<TokenKeyBinding?> UpdatePublicKeyAsync(
        Guid tokenId,
        string signingKeyId,
        string newPublicKey)
    {
        var binding = await repository.GetAsync(
            tokenId,
            signingKeyId);

        if (binding is null)
            return null;

        var updated = binding.UpdatePublicKey(newPublicKey);

        await repository.UpdateAsync(updated);
        return updated;
    }

    /// <summary>
    /// Revokes all active key bindings associated with a developer token.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <returns>
    /// <c>true</c> if at least one binding was revoked; otherwise,
    /// <c>false</c> when no active bindings were found.
    /// </returns>
    public async Task<bool> RevokeAsync(Guid tokenId)
    {
        var bindings = await repository.ListByTokenAsync(tokenId);
        var anyUpdated = false;

        foreach (var binding in bindings)
        {
            if (binding.Revoked)
                continue;

            var revoked = binding.Revoke();

            await repository.UpdateAsync(revoked);

            anyUpdated = true;
        }

        return anyUpdated;
    }

    /// <summary>
    /// Retrieves all key bindings associated with a developer token.
    /// </summary>
    /// <param name="tokenId">
    /// The unique identifier of the developer token.
    /// </param>
    /// <returns>
    /// A collection of <see cref="TokenKeyBinding"/> entities associated
    /// with the specified token.
    /// </returns>
    public Task<IEnumerable<TokenKeyBinding>> ListBindingsAsync(Guid tokenId) =>
        repository.ListByTokenAsync(tokenId);

    /// <summary>
    /// Retrieves specific key binding for developer token and signing key.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token. </param>
    /// <param name="signingKeyId"> The identifier of the signing key to retrieve. </param>
    /// <returns>
    /// The matching <see cref="TokenKeyBinding"/>, or <c>null</c> if no
    /// matching binding exists.
    /// </returns>
    public Task<TokenKeyBinding?> GetBindingAsync(Guid tokenId, string signingKeyId) =>
        repository.GetAsync(tokenId, signingKeyId);
}