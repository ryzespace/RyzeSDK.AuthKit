using Application.DTO;
using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Provides operations for managing developer tokens.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Handles creation, deletion, and retrieval of developer tokens.</item>
/// <item>Abstracts away repository operations and token signing.</item>
/// </list>
/// </remarks>
public interface IDeveloperTokenManager
{
    /// <summary>
    /// Creates a new developer token and persists it using the repository.
    /// </summary>
    /// <param name="developerId">The unique identifier of the developer.</param>
    /// <param name="name">The name of the token.</param>
    /// <param name="scopes">A collection of scopes associated with the token.</param>
    /// <param name="lifetime">Optional token lifetime.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A DTO representing the created developer token, including its signed JWT.</returns>
     Task<DeveloperTokenCreated> CreateAsync(
        Guid developerId,
        string name,
        IEnumerable<string> scopes,
        TimeSpan? lifetime = null,
        CancellationToken ct = default);
     
     /// <summary>
     /// Deletes an existing token by its unique identifier.
     /// </summary>
     /// <param name="tokenId">The unique identifier of the token to delete.</param>
     /// <param name="ct">Optional cancellation token.</param>
     Task DeleteAsync(Guid tokenId, CancellationToken ct = default);
     
     /// <summary>
     /// Retrieves all tokens associated with a specific developer.
     /// </summary>
     /// <param name="developerId">The developer’s unique identifier.</param>
     /// <param name="ct">Optional cancellation token.</param>
     /// <returns>A read-only list of token DTOs.</returns>
     Task<IReadOnlyList<DeveloperToken>> GetByDeveloperAsync(Guid developerId, CancellationToken ct = default);
     
     /// <summary>
     /// Retrieves a token by its unique ID.
     /// </summary>
     /// <param name="id">The unique identifier of the token.</param>
     /// <param name="ct">Optional <see cref="CancellationToken"/>.</param>
     /// <returns>The <see cref="DeveloperToken"/> if found; otherwise, <c>null</c>.</returns>
     Task<DeveloperToken?> GetByIdAsync(Guid id, CancellationToken ct = default);
     
     
}