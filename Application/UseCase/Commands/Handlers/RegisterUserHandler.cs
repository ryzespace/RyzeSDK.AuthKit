using Application.DTO;
using Application.Interfaces;
using Application.UseCase.Commands.Requests;

namespace Application.UseCase.Commands.Handlers;

/// <summary>
/// Handler responsible for processing <see cref="RegisterUserCommand"/> 
/// by creating a new user in Keycloak via <see cref="IKeycloakService"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Maps <see cref="RegisterUserCommand"/> to <see cref="KeycloakUserDto"/>.</description></item>
/// <item><description>Delegates user creation to <see cref="IKeycloakService.CreateUserAsync"/>.</description></item>
/// </list>
/// </remarks>
public class RegisterUserHandler(IKeycloakService keycloak)
{
    /// <summary>
    /// Handles the registration command by creating a user in Keycloak.
    /// </summary>
    /// <param name="command">The <see cref="RegisterUserCommand"/> containing user registration data.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var user = new KeycloakUserDto(
            command.FirstName,
            command.LastName,
            command.Username,
            command.Email,
            command.Password
        );
        
        await keycloak.CreateUserAsync(user, cancellationToken);
    }
}