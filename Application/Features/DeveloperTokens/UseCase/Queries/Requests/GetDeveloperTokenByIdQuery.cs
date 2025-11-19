namespace Application.Features.DeveloperTokens.UseCase.Queries.Requests;

/// <summary>
/// Query for retrieving a specific developer token by its unique identifier.
/// </summary>
/// <param name="TokenId">The unique identifier of the token to retrieve.</param>
public record GetDeveloperTokenByIdQuery(
    Guid TokenId
);