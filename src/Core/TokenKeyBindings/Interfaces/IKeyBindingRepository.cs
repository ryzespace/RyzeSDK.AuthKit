using Core.TokenKeyBindings.Services;

namespace Core.TokenKeyBindings.Interfaces;

/// <summary>
/// Defines repository abstraction for persisting and retrieving
/// <see cref="TokenKeyBinding"/> entities.
/// </summary>
/// <remarks>
/// <para>
/// Provides asynchronous persistence operations for token to signing key bindings.
/// Supports adding, retrieving, updating, and listing bindings associated
/// with developer tokens and signing keys.
/// </para>
/// </remarks>
public interface IKeyBindingRepository
{
    /// <summary>
    /// Adds a new <see cref="TokenKeyBinding"/> to the repository.
    /// </summary>
    /// <param name="binding">The key binding entity to add.</param>
    /// <returns>The persisted <see cref="TokenKeyBinding"/> entity.</returns>
    Task<TokenKeyBinding> AddAsync(TokenKeyBinding binding);

    /// <summary>
    /// Retrieves a key binding by token ID and signing key ID.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <param name="signingKeyId">The ID of the signing key associated with the token.</param>
    /// <returns>The <see cref="TokenKeyBinding"/> if found; otherwise, <c>null</c>.</returns>
    Task<TokenKeyBinding?> GetAsync(Guid tokenId, string signingKeyId);

    /// <summary>
    /// Updates an existing <see cref="TokenKeyBinding"/> in the repository.
    /// </summary>
    /// <param name="binding">The key binding entity to update.</param>
    Task UpdateAsync(TokenKeyBinding binding);

    /// <summary>
    /// Lists all key bindings associated with a specific token.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <returns>A collection of <see cref="TokenKeyBinding"/> entities.</returns>
    Task<IEnumerable<TokenKeyBinding>> ListByTokenAsync(Guid tokenId);
}
