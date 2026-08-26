using Core.TokenKeyBindings.Interfaces;
using Core.TokenKeyBindings.Services;
using Marten;

namespace Host.TokenKeyBindings.Repositories;

/// <summary>
/// Provides Marten based repository for persisting
/// <see cref="TokenKeyBinding"/> entities.
/// </summary>
/// <remarks>
/// <para>
/// Each binding is stored as a single Marten document identified by a composite
/// document id built from the developer token id and the signing key id. Only the
/// serialized binding is persisted; the repository performs no domain logic.
/// </para>
/// </remarks>
public sealed class KeyBindingRepository(IDocumentStore store) : IKeyBindingRepository
{
    /// <summary>
    /// Builds the fixed Marten document identifier fo binding.
    /// </summary>
    private static string DocumentId(Guid tokenId, string signingKeyId) =>
        $"{tokenId:N}:{signingKeyId}";

    /// <summary>
    /// adds new key binding to Marten.
    /// </summary>
    /// <param name="binding">The key binding to persist.</param>
    /// <returns>The persisted <see cref="TokenKeyBinding"/>.</returns>
    public async Task<TokenKeyBinding> AddAsync(TokenKeyBinding binding)
    {
        await using var session = store.LightweightSession();

        session.Store(new KeyBindingDocument
        {
            Id = DocumentId(binding.TokenId, binding.SigningKeyId),
            Binding = binding
        });

        await session.SaveChangesAsync();
        return binding;
    }

    /// <summary>
    /// retrieves key binding by token id and signing key id.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <param name="signingKeyId">The identifier of the signing key.</param>
    /// <returns>
    /// The matching <see cref="TokenKeyBinding"/>, or <c>null</c> when none exists.
    /// </returns>
    public async Task<TokenKeyBinding?> GetAsync(Guid tokenId, string signingKeyId)
    {
        await using var session = store.LightweightSession();
        var document = await session.LoadAsync<KeyBindingDocument>(
            DocumentId(tokenId, signingKeyId));

        return document?.Binding;
    }

    /// <summary>
    /// updates an existing key binding in Marten.
    /// </summary>
    /// <param name="binding">The updated key binding.</param>
    public async Task UpdateAsync(TokenKeyBinding binding)
    {
        await using var session = store.LightweightSession();

        session.Store(new KeyBindingDocument
        {
            Id = DocumentId(binding.TokenId, binding.SigningKeyId),
            Binding = binding
        });

        await session.SaveChangesAsync();
    }

    /// <summary>
    /// lists all key bindings associated with a developer token.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <returns>All <see cref="TokenKeyBinding"/> entities for the token.</returns>
    public async Task<IEnumerable<TokenKeyBinding>> ListByTokenAsync(Guid tokenId)
    {
        await using var session = store.LightweightSession();

        var documents = await session.Query<KeyBindingDocument>()
            .Where(document => document.Binding.TokenId == tokenId)
            .ToListAsync();

        return documents.Select(document => document.Binding);
    }

    /// <summary>
    /// Represents the persisted Marten document containing a single
    /// <see cref="TokenKeyBinding"/>.
    /// </summary>
    /// <remarks>
    /// The document intentionally contains only the binding payload. The
    /// repository does not perform domain transitions; those live on the
    /// <see cref="TokenKeyBinding"/> entity itself.
    /// </remarks>
    public sealed class KeyBindingDocument
    {
        /// <summary>
        /// Gets or sets the Marten document identifier.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Gets or sets the persisted binding.
        /// </summary>
        public TokenKeyBinding Binding { get; set; } = null!;
    }
}
