using Marten;

namespace DevTokens.Repositories;

/// <summary>
/// Repository for managing <see cref="DeveloperToken"/> persistence using Marten.
/// </summary>
/// <remarks>
/// <para>
/// Provides persistence operations for developer tokens through a Marten
/// <see cref="IDocumentSession"/>.
/// </para>
/// <list type="bullet">
/// <item>Stores and updates developer tokens in the document database.</item>
/// <item>Deletes developer tokens by their unique identifier.</item>
/// <item>Retrieves individual tokens by ID.</item>
/// <item>Retrieves all tokens belonging to a specific developer.</item>
/// <item>Supports cancellation for all asynchronous database operations.</item>
/// </list>
/// </remarks>
public class DeveloperTokenRepository(IDocumentSession session) : IDeveloperTokenRepository
{
    /// <summary>
    /// Saves the specified developer token to the document database.
    /// </summary>
    /// <param name="token">The developer token to persist.</param>
    /// <param name="ct">A token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous persistence operation.</returns>
    public async Task SaveAsync(
        DeveloperToken token,
        CancellationToken ct = default)
    {
        session.Store(token);
        await session.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Deletes the developer token with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the token to delete.</param>
    /// <param name="ct">A token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous deletion operation.</returns>
    public async Task DeleteAsync(
        Guid id,
        CancellationToken ct = default)
    {
        session.Delete<DeveloperToken>(id);
        await session.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Retrieves developer token by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the token.</param>
    /// <param name="ct">A token that can be used to cancel the operation.</param>
    /// <returns>The matching <see cref="DeveloperToken"/> when found otherwise, <c>null</c>. </returns>
    public async Task<DeveloperToken?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
        => await session.LoadAsync<DeveloperToken>(id, ct);

    /// <summary>
    /// Retrieves all developer tokens associated with the specified developer.
    /// </summary>
    /// <param name="developerId">The unique identifier of the developer.</param>
    /// <param name="ct">A token that can be used to cancel the operation.</param>
    /// <returns>A readonly list containing all tokens associated with the specified developer.</returns>
    public async Task<IReadOnlyList<DeveloperToken>> GetByDeveloperIdAsync(
        Guid developerId,
        CancellationToken ct = default)
        => await session.Query<DeveloperToken>()
            .Where(x => x.DeveloperId == developerId)
            .ToListAsync(ct);
}