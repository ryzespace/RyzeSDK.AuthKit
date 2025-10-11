namespace Application.UseCase.Commands.Requests;

/// <summary>
/// Command representing a request to register a new user in the system.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Contains all necessary information to create a new user account.</description></item>
/// <item><description>Validated by <see cref="Application.UseCase.Commands.Validations.RegisterUserCommandValidator"/> before processing.</description></item>
/// </list>
/// </remarks>
/// <param name="FirstName">The first name of the user.</param>
/// <param name="LastName">The last name of the user.</param>
/// <param name="Username">The unique username for the user.</param>
/// <param name="Email">The email address of the user.</param>
/// <param name="Password">The password for the user account.</param>
public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Username,
    string Email,
    string Password
);