using System.Diagnostics;
using Domain.Features.TokenKeyBindings;

namespace Infrastructure.Persistence.KeyBinding;

/// <summary>
/// In-memory repository for <see cref="TokenKeyBinding"/> entities.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Primarily used for testing, local development, or scenarios without a persistent database.</item>
/// <item>Thread-safe via internal locking on all operations.</item>
/// <item>Logs all operations for debugging and traceability.</item>
/// </list>
/// </remarks>
public class InMemoryKeyBindingRepository : IKeyBindingRepository
{
    private readonly List<TokenKeyBinding> _store = [];
    private readonly Lock _lock = new();

    private static void DebugLog(string message)
    {
        Debug.WriteLine($"[KeyBindingRepo] {message}");
    }

    public Task<TokenKeyBinding> AddAsync(TokenKeyBinding binding)
    {
        lock (_lock)
        {
            _store.Add(binding);
        }
        DebugLog($"Added binding: TokenId={binding.TokenId}, SigningKeyId={binding.SigningKeyId}");
        return Task.FromResult(binding);
    }

    public Task<TokenKeyBinding?> GetAsync(Guid tokenId, string signingKeyId)
    {
        TokenKeyBinding? found;
        lock (_lock)
        {
            found = _store.FirstOrDefault(b => b.TokenId == tokenId && b.SigningKeyId == signingKeyId);
        }
        DebugLog(found != null
            ? $"Found binding for TokenId={tokenId}, SigningKeyId={signingKeyId}"
            : $"No binding found for TokenId={tokenId}, SigningKeyId={signingKeyId}");
        return Task.FromResult(found);
    }

    public Task UpdateAsync(TokenKeyBinding binding)
    {
        lock (_lock)
        {
            var index = _store.FindIndex(b => b.TokenId == binding.TokenId && b.SigningKeyId == binding.SigningKeyId);
            if (index >= 0)
            {
                _store[index] = binding;
                DebugLog($"Updated binding: TokenId={binding.TokenId}, SigningKeyId={binding.SigningKeyId}");
            }
            else
            {
                DebugLog($"Attempted to update non-existing binding: TokenId={binding.TokenId}, SigningKeyId={binding.SigningKeyId}");
            }
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<TokenKeyBinding>> ListByTokenAsync(Guid tokenId)
    {
        IEnumerable<TokenKeyBinding> result;
        lock (_lock)
        {
            result = _store.Where(b => b.TokenId == tokenId).ToList();
        }
        DebugLog($"Listed {result.Count()} bindings for TokenId={tokenId}");
        return Task.FromResult(result);
    }
}