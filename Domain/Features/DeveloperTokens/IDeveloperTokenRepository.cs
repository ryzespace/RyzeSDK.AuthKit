namespace Domain.Features.DeveloperTokens;

/// <summary>
/// Repository interface for managing <see cref="DeveloperToken"/> entities.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Handles persistence and retrieval of developer tokens.</item>
/// <item>Supports saving, deleting, and querying tokens by ID or developer ID.</item>
/// <item>All methods are asynchronous and support <see cref="CancellationToken"/>.</item>
/// </list>
/// </remarks>
public interface IDeveloperTokenRepository
{
    /// <summary>
    /// Saves the specified <see cref="DeveloperToken"/> to the repository.
    /// </summary>
    /// <param name="token">The token to save.</param>
    /// <param name="ct">Optional <see cref="CancellationToken"/>.</param>
    Task SaveAsync(DeveloperToken token, CancellationToken ct = default);

    /// <summary>
    /// Deletes the token with the specified ID from the repository.
    /// </summary>
    /// <param name="id">The unique identifier of the token to delete.</param>
    /// <param name="ct">Optional <see cref="CancellationToken"/>.</param>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a token by its unique ID.
    /// </summary>
    /// <param name="id">The unique identifier of the token.</param>
    /// <param name="ct">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="DeveloperToken"/> if found; otherwise, <c>null</c>.</returns>
    Task<DeveloperToken?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all tokens associated with a specific developer.
    /// </summary>
    /// <param name="developerId">The unique identifier of the developer.</param>
    /// <param name="ct">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>A read-only list of <see cref="DeveloperToken"/> instances.</returns>
    Task<IReadOnlyList<DeveloperToken>> GetByDeveloperIdAsync(Guid developerId, CancellationToken ct = default);
}