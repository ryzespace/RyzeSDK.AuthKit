namespace Application.Features.DeveloperTokens.UseCase.Commands.Requests;

/// <summary>
/// Command for creating a new developer token.
/// </summary>
/// <param name="DeveloperId">The unique identifier of the developer for whom the token is being created.</param>
/// <param name="Name">The name of the token.</param>
/// <param name="Description">Optional description of the token.</param>
/// <param name="Scopes">A collection of scopes associated with the token.</param>
/// <param name="Lifetime">Optional token lifetime. If null, the token does not expire.</param>
public record CreateDeveloperTokenCommand(
    Guid DeveloperId,
    string Name,
    string Description,
    IEnumerable<string> Scopes,
    TimeSpan? Lifetime
);