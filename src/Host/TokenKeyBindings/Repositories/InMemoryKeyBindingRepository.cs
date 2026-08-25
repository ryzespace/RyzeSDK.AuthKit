using System.Diagnostics;
using Core.TokenKeyBindings;
using Core.TokenKeyBindings.Interfaces;
using Core.TokenKeyBindings.Services;

namespace Host.TokenKeyBindings.Repositories;

/// <summary>
/// Provides an in-memory implementation of <see cref="IKeyBindingRepository"/>
/// for storing <see cref="TokenKeyBinding"/> entities.
/// </summary>
/// <remarks>
/// <para>
/// Stores key bindings in process memory and is primarily intended for testing,
/// local development, and scenarios that do not require persistent storage.
/// </para>
/// <para>
/// All repository operations are synchronized using an internal lock to ensure
/// thread-safe access to the in-memory collection.
/// </para>
/// <para>
/// Repository operations are written to the debug output for diagnostics and
/// development purposes.
/// </para>
/// </remarks>
public class InMemoryKeyBindingRepository : IKeyBindingRepository
{
    private readonly List<TokenKeyBinding> _store = [];
    private readonly Lock _lock = new();

    /// <summary>
    /// Writes repository diagnostic message to the debug output.
    /// </summary>
    /// <param name="message">The diagnostic message to write.</param>
    private static void DebugLog(string message)
        => Debug.WriteLine($"[KeyBindingRepo] {message}");

    /// <summary>
    /// Adds new key binding to the in-memory store.
    /// </summary>
    /// <param name="binding">The key binding to store.</param>
    /// <returns>
    /// A task containing the stored <see cref="TokenKeyBinding"/>.
    /// </returns>
    public Task<TokenKeyBinding> AddAsync(TokenKeyBinding binding)
    {
        lock (_lock)
        {
            _store.Add(binding);
        }

        DebugLog(
            $"Added binding: TokenId={binding.TokenId}, SigningKeyId={binding.SigningKeyId}");

        return Task.FromResult(binding);
    }

    /// <summary>
    /// Retrieves key binding by developer token ID and signing key ID.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <param name="signingKeyId">The identifier of the signing key.</param>
    /// <returns>
    /// A task containing the matching <see cref="TokenKeyBinding"/>,
    /// or <c>null</c> when no matching binding exists.
    /// </returns>
    public Task<TokenKeyBinding?> GetAsync(
        Guid tokenId,
        string signingKeyId)
    {
        TokenKeyBinding? found;

        lock (_lock)
        {
            found = _store.FirstOrDefault(
                binding =>
                    binding.TokenId == tokenId &&
                    binding.SigningKeyId == signingKeyId);
        }

        DebugLog(
            found != null
                ? $"Found binding for TokenId={tokenId}, SigningKeyId={signingKeyId}"
                : $"No binding found for TokenId={tokenId}, SigningKeyId={signingKeyId}");

        return Task.FromResult(found);
    }

    /// <summary>
    /// Updates an existing key binding in the in-memory store.
    /// </summary>
    /// <param name="binding">The updated key binding.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public Task UpdateAsync(TokenKeyBinding binding)
    {
        lock (_lock)
        {
            var index = _store.FindIndex(
                existing =>
                    existing.TokenId == binding.TokenId &&
                    existing.SigningKeyId == binding.SigningKeyId);

            if (index >= 0)
            {
                _store[index] = binding;

                DebugLog(
                    $"Updated binding: TokenId={binding.TokenId}, SigningKeyId={binding.SigningKeyId}");
            }
            else
            {
                DebugLog(
                    $"Attempted to update non-existing binding: TokenId={binding.TokenId}, SigningKeyId={binding.SigningKeyId}");
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Lists all key bindings associated with a developer token.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token.</param>
    /// <returns>
    /// A task containing all <see cref="TokenKeyBinding"/> entities associated
    /// with the specified developer token.
    /// </returns>
    public Task<IEnumerable<TokenKeyBinding>> ListByTokenAsync(Guid tokenId)
    {
        IEnumerable<TokenKeyBinding> result;

        lock (_lock)
        {
            result = [.. _store.Where(binding => binding.TokenId == tokenId)];
        }

        DebugLog($"Listed {result.Count()} bindings for TokenId={tokenId}");

        return Task.FromResult(result);
    }
}
