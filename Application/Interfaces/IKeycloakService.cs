using Application.DTO;

namespace Application.Interfaces;

/// <summary>
/// Defines high-level Keycloak user management operations for the application layer.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Provides methods for creating, updating, and deleting users using <see cref="KeycloakUserDto"/>.</description></item>
/// <item><description>Abstracts the underlying Keycloak infrastructure (providers and repositories) from application logic.</description></item>
/// <item><description>Ensures operations are executed asynchronously with cancellation support.</description></item>
/// </list>
/// </remarks>
public interface IKeycloakService
{
    /// <summary>
    /// Creates a new user in Keycloak based on the provided DTO.
    /// </summary>
    /// <param name="dtoUser">The user data transfer object containing user details.</param>
    /// <param name="cancellationToken">Optional cancellation token for the asynchronous operation.</param>
    Task CreateUserAsync(KeycloakUserDto dtoUser, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing Keycloak user based on the provided DTO.
    /// </summary>
    /// <param name="dtoUser">The user data transfer object containing updated user details.</param>
    /// <param name="cancellationToken">Optional cancellation token for the asynchronous operation.</param>
    Task UpdateUserAsync(KeycloakUserDto dtoUser, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a Keycloak user identified by the specified email address.
    /// </summary>
    /// <param name="email">The email of the user to delete.</param>
    /// <param name="cancellationToken">Optional cancellation token for the asynchronous operation.</param>
    Task DeleteUserAsync(string email, CancellationToken cancellationToken);
}