namespace Application.UseCase.Commands.Requests;

/// <summary>
/// Represents a command to delete a specific developer token.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Used to request the removal of a <c>DeveloperToken</c> by its unique identifier.</item>
/// <item>Handled by a corresponding command handler in the application layer.</item>
/// </list>
/// </remarks>
public record DeleteTokenCommand(
    Guid TokenId
);