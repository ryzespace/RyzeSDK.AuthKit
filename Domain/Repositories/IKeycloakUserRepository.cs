using NETCore.Keycloak.Client.Models.Users;

namespace Domain.Repositories;

/// <summary>
/// Defines repository operations for managing Keycloak users through the admin API.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Provides command operations for creating, updating, and deleting users in Keycloak.</description></item>
/// <item><description>Supports query operations for retrieving and validating user data by email or username.</description></item>
/// <item><description>Abstracts direct communication with the Keycloak REST API for user management.</description></item>
/// </list>
/// </remarks>
public interface IKeycloakUserRepository
{
    /// <summary>
    /// Creates a new user in the Keycloak realm using the admin API.
    /// </summary>
    /// <param name="user">The user model containing user attributes to create.</param>
    /// <param name="accessToken">The access token authorizing the request.</param>
    /// <param name="cancellationToken">Optional cancellation token for the asynchronous operation.</param>
    Task CreateUserAsync(KcUser user, string accessToken, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing user in the Keycloak realm.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to update.</param>
    /// <param name="user">The user model containing updated attributes.</param>
    /// <param name="accessToken">The access token authorizing the request.</param>
    /// <param name="cancellationToken">Optional cancellation token for the asynchronous operation.</param>
    Task UpdateUserAsync(string userId, KcUser user, string accessToken, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a user from the Keycloak realm.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete.</param>
    /// <param name="accessToken">The access token authorizing the request.</param>
    /// <param name="cancellationToken">Optional cancellation token for the asynchronous operation.</param>
    Task DeleteUserAsync(string userId, string accessToken, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a user exists in Keycloak by the provided email address.
    /// </summary>
    /// <param name="email">The email address to check for existence.</param>
    /// <param name="accessToken">The access token authorizing the request.</param>
    /// <param name="cancellationToken">Optional cancellation token for the asynchronous operation.</param>
    /// <returns><see langword="true"/> if a user with the specified email exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> UserExistsByEmailAsync(string email, string accessToken, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a user from Keycloak by their email address.
    /// </summary>
    /// <param name="email">The email address of the user to retrieve.</param>
    /// <param name="accessToken">The access token authorizing the request.</param>
    /// <param name="cancellationToken">Optional cancellation token for the asynchronous operation.</param>
    /// <returns>The <see cref="KcUser"/> if found; otherwise, <see langword="null"/>.</returns>
    Task<KcUser?> GetUserByEmailAsync(string email, string accessToken, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a user from Keycloak by their username.
    /// </summary>
    /// <param name="username">The username of the user to retrieve.</param>
    /// <param name="accessToken">The access token authorizing the request.</param>
    /// <param name="cancellationToken">Optional cancellation token for the asynchronous operation.</param>
    /// <returns>The <see cref="KcUser"/> if found; otherwise, <see langword="null"/>.</returns>
    Task<KcUser?> GetUserByUsernameAsync(string username, string accessToken, CancellationToken cancellationToken);
}
