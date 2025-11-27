using Domain.Features.DeveloperTokens;
using Domain.Features.DeveloperTokens.Repositories;
using Marten;

namespace Infrastructure.Repositories.DeveloperTokens;

/// <summary>
/// Repository for managing <see cref="DeveloperToken"/> persistence using Marten.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Stores, deletes, and retrieves developer tokens from the document database.</item>
/// <item>Implements <see cref="IDeveloperTokenRepository"/>.</item>
/// </list>
/// </remarks>
public class DeveloperTokenRepository(IDocumentSession session) : IDeveloperTokenRepository
{
    /// <inheritdoc />
    public async Task SaveAsync(DeveloperToken token, CancellationToken ct = default)
    {
        session.Store(token);
        await session.SaveChangesAsync(ct);
    }
    
    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        session.Delete<DeveloperToken>(id);
        await session.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<DeveloperToken?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await session.LoadAsync<DeveloperToken>(id, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<DeveloperToken>> GetByDeveloperIdAsync(Guid developerId, CancellationToken ct = default)
        => await session.Query<DeveloperToken>()
            .Where(x => x.DeveloperId == developerId)
            .ToListAsync(ct);
}