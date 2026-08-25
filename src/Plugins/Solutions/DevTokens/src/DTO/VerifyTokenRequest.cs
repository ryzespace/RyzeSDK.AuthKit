namespace DevTokens.DTO;

/// <summary>
/// Represents a request to verify the developer token using its short key.
/// </summary>
/// <param name="Key">
/// The developer token key to verify.
/// </param>
public record VerifyTokenRequest(
    string Key
);