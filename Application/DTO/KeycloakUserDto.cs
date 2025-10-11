namespace Application.DTO;

/// <summary>
/// Data Transfer Object representing a user for Keycloak operations.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Contains all necessary fields for creating or updating a Keycloak user.</description></item>
/// <item><description>Used by <see cref="IKeycloakService"/> methods for user management.</description></item>
/// </list>
/// </remarks>
/// <param name="FirstName">The first name of the user.</param>
/// <param name="LastName">The last name of the user.</param>
/// <param name="Username">The unique username of the user.</param>
/// <param name="Email">The email address of the user.</param>
/// <param name="Password">The password for the user account.</param>
public record KeycloakUserDto(
    string FirstName,
    string LastName,
    string Username,
    string Email,
    string Password
);