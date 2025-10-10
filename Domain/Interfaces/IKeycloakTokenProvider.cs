namespace Domain.Interfaces;

/// <summary>
/// Defines a contract for retrieving Keycloak access tokens.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Abstracts token retrieval logic from the Keycloak authentication server.</description></item>
/// <item><description>Used by infrastructure services to securely get access tokens for API calls.</description></item>
/// <item><description>Encapsulates client credentials flow to keep application code decoupled from Keycloak internals.</description></item>
/// </list>
/// </remarks>
public interface IKeycloakTokenProvider
{
    /// <summary>
    /// Retrieves a valid Keycloak access token using the configured client credentials flow.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token for the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation,
    /// returning the access token string for authenticated requests.
    /// </returns>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}