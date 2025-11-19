namespace Infrastructure.Restful.DeveloperTokens.DTO;

public record CreateTokenRequest(
    string Name,
    string Description,
    IEnumerable<string> Scopes,
    int? LifetimeDays
);