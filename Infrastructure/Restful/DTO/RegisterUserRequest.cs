namespace Infrastructure.Restful.DTO;

/// <summary>
/// DTO representing a request to register a new user via REST API.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Contains all necessary information to create a new user account.</description></item>
/// <item><description>Used in <see cref="Infrastructure.Restful.Controllers.AuthController"/> endpoints.</description></item>
/// </list>
/// </remarks>
/// <param name="FirstName">The first name of the new user.</param>
/// <param name="LastName">The last name of the new user.</param>
/// <param name="Username">The desired username of the new user.</param>
/// <param name="Email">The email address of the new user.</param>
/// <param name="Password">The password for the new user account.</param>
public record RegisterUserRequest(
    string FirstName,
    string LastName,
    string Username,
    string Email,
    string Password
);