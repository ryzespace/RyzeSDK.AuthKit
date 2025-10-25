namespace Application.UseCase.Commands.Requests;

public record CreateDeveloperTokenCommand(
    Guid DeveloperId,
    string Name,
    string Description,
    IEnumerable<string> Scopes,
    TimeSpan? Lifetime
);