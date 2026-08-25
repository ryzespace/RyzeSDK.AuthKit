using DevTokens.DTO;
using DevTokens.Interfaces;
using DevTokens.Repositories;
using DevTokens.ValueObject;

namespace DevTokens.Services;

/// <summary>
/// Manages the lifecycle of developer tokens.
/// </summary>
/// <remarks>
/// <para>
/// Provides operations for creating, deleting, and retrieving developer tokens.
/// Delegates token generation to <see cref="IDeveloperTokenService"/>
/// and token persistence to <see cref="IDeveloperTokenRepository"/>.
/// </para>
/// </remarks>
public class DeveloperTokenManager(
    IDeveloperTokenService tokenService,
    IDeveloperTokenRepository repository) : IDeveloperTokenManager
{
    /// <summary>
    /// Creates a new developer token and generates its token credentials.
    /// </summary>
    /// <param name="developerId">The unique identifier of the developer who owns the token.</param>
    /// <param name="name">The name assigned to the token.</param>
    /// <param name="scopes">The scopes granted to the token.</param>
    /// <param name="lifetime">
    /// Optional duration for which the token remains valid. If <c>null</c>,
    /// the token does not expire.
    /// </param>
    /// <param name="ct">Cancellation token for the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="DeveloperTokenCreated"/> containing the created token,
    /// its short API key, and signed JWT.
    /// </returns>
    public async Task<DeveloperTokenCreated> CreateAsync(
        Guid developerId,
        string name,
        IEnumerable<string> scopes,
        TimeSpan? lifetime = null,
        CancellationToken ct = default)
    {
        var token = DeveloperToken.Create(
            developerId,
            name,
            scopes.Select(s => (TokenScope)s),
            lifetime
        );

        var tokenPair = await tokenService.GenerateToken(token);

        return new DeveloperTokenCreated
        {
            Token = token,
            ShortKey = tokenPair.ShortKey,
            Jwt = tokenPair.Jwt
        };
    }

    /// <summary>
    /// Deletes developer token by its unique identifier.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the token to delete.</param>
    /// <param name="ct">Cancellation token for the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous deletion operation.</returns>
    public async Task DeleteAsync(
        Guid tokenId,
        CancellationToken ct = default) =>
        await repository.DeleteAsync(tokenId, ct);

    /// <summary>
    /// Retrieves all developer tokens belonging to the specified developer.
    /// </summary>
    /// <param name="developerId">The unique identifier of the developer.</param>
    /// <param name="ct">Cancellation token for the asynchronous operation.</param>
    /// <returns>A readonly list containing the developer's tokens. </returns>
    public async Task<IReadOnlyList<DeveloperToken>> GetByDeveloperAsync(
        Guid developerId,
        CancellationToken ct = default) =>
        await repository.GetByDeveloperIdAsync(developerId, ct);

    /// <summary>
    /// Retrieves developer token by its unique identifier.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the token.</param>
    /// <param name="ct">Cancellation token for the asynchronous operation.</param>
    /// <returns>The matching <see cref="DeveloperToken"/>, or <c>null</c> if the token does not exist. </returns>
    public async Task<DeveloperToken?> GetByIdAsync(
        Guid tokenId,
        CancellationToken ct = default) =>
        await repository.GetByIdAsync(tokenId, ct);
}