using Application.DTO;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObject;

namespace Application.Services;

/// <summary>
/// Manager responsible for handling developer token operations.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Provides creation, deletion, and retrieval of developer tokens.</item>
/// <item>Delegates JWT generation to <see cref="IDeveloperTokenService"/>.</item>
/// <item>Persists tokens using <see cref="IDeveloperTokenRepository"/>.</item>
/// </list>
/// </remarks>
public class DeveloperTokenManager(
    IDeveloperTokenService tokenService,
    IDeveloperTokenRepository repository
    ) : IDeveloperTokenManager
{
    /// <inheritdoc />
    public async Task<DeveloperTokenCreated> CreateAsync(Guid developerId, string name, IEnumerable<string> scopes, TimeSpan? lifetime = null,
        CancellationToken ct = default)
    {
        var token = DeveloperToken.Create(
            developerId,
            name,
            scopes.Select(s => (TokenScope)s),
            lifetime
        );

        await repository.SaveAsync(token, ct);
        var jwt = await tokenService.GenerateToken(token);

        return new DeveloperTokenCreated
        {
            Token = token,
            Jwt = jwt
        };
    }
    
    /// <inheritdoc />
    public async Task DeleteAsync(Guid tokenId, CancellationToken ct = default) =>
        await repository.DeleteAsync(tokenId, ct);
    
    /// <inheritdoc />
    public async Task<IReadOnlyList<DeveloperToken>> GetByDeveloperAsync(Guid developerId, CancellationToken ct = default) =>
        await repository.GetByDeveloperIdAsync(developerId, ct);
    
    /// <inheritdoc />
    public async Task<DeveloperToken?> GetByIdAsync(Guid tokenId, CancellationToken ct = default) =>
        await repository.GetByIdAsync(tokenId, ct);
}