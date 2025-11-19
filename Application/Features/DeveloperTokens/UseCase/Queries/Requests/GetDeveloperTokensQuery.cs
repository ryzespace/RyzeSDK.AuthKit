namespace Application.Features.DeveloperTokens.UseCase.Queries.Requests;

/// <summary>
/// Query for retrieving all developer tokens associated with a specific developer.
/// </summary>
/// <param name="DeveloperId">The unique identifier of the developer whose tokens should be retrieved.</param>
public record GetDeveloperTokensQuery(
    Guid DeveloperId
);