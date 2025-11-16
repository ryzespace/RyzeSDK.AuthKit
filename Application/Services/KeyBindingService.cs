using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Services;

/// <summary>
/// Service responsible for managing key bindings between <see cref="DeveloperToken"/> instances and RSA signing keys.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Creates new key bindings linking a developer token to a signing key.</item>
/// <item>Supports rebinding an existing token to a new signing key.</item>
/// <item>Allows updating public keys without changing the signing key ID.</item>
/// <item>Handles revocation of key bindings and listing or retrieving bindings by token ID.</item>
/// </list>
/// </remarks>
public class KeyBindingService(IKeyBindingRepository repository) : IKeyBindingService
{
    /// <inheritdoc />
    public Task<TokenKeyBinding> CreateBindingAsync(Guid tokenId, string signingKeyId, string publicKey)
    {
        var binding = new TokenKeyBinding(tokenId, signingKeyId, publicKey);
        return repository.AddAsync(binding);
    }

    /// <inheritdoc />
    public async Task<TokenKeyBinding?> RebindAsync(Guid tokenId, string signingKeyId, string newSigningKeyId, string newPublicKey)
    {
        var binding = await repository.GetAsync(tokenId, signingKeyId);
        if (binding == null) return null;

        var updated = binding.Rebind(newSigningKeyId, newPublicKey);
        await repository.UpdateAsync(updated);
        return updated;
    }

    /// <inheritdoc />
    public async Task<TokenKeyBinding?> UpdatePublicKeyAsync(Guid tokenId, string signingKeyId, string newPublicKey)
    {
        var binding = await repository.GetAsync(tokenId, signingKeyId);
        if (binding == null) return null;

        var updated = binding.UpdatePublicKey(newPublicKey);
        await repository.UpdateAsync(updated);
        return updated;
    }
    /// <inheritdoc />
    public async Task<bool> RevokeAsync(string tokenId)
    {
        var bindings = await repository.ListByTokenAsync(Guid.Parse(tokenId));
        var anyUpdated = false;

        foreach (var b in bindings)
        {
            if (b.Revoked) continue;
            var revoked = b.Revoke();
            await repository.UpdateAsync(revoked);
            anyUpdated = true;
        }

        return anyUpdated;
    }


    /// <inheritdoc />
    public Task<IEnumerable<TokenKeyBinding>> ListBindingsAsync(Guid tokenId)
        => repository.ListByTokenAsync(tokenId);
    
    /// <inheritdoc />
    public Task<TokenKeyBinding?> GetBindingAsync(Guid tokenId, string signingKeyId)
        => repository.GetAsync(tokenId, signingKeyId);
}