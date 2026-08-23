namespace DevTokens.DTO;

/// <summary>
/// Represents request to create new developer token.
/// </summary>
/// <param name="Name">The name assigned to the developer token.</param>
/// <param name="Description">A description of the developer token and its intended use.</param>
/// <param name="Scopes">The scopes granted to the developer token.</param>
/// <param name="LifetimeDays">
/// The optional lifetime of the developer token in days.
/// A <c>null</c> value indicates that the default lifetime should be used.
/// </param>
public record CreateTokenRequest(
    string Name,
    string Description,
    IEnumerable<string> Scopes,
    int? LifetimeDays
);