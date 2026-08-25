namespace DevTokens.DTO;

/// <summary>
/// Represents the generated credentials for developer token.
/// </summary>
/// <remarks>
/// <para>
/// Contains both the short API key used to identify the token and the
/// signed JWT used for authenticated requests.
/// </para>
/// </remarks>
/// <param name="ShortKey">The public identifier used as an API key in requests. </param>
/// <param name="Jwt">The signed JSON Web Token containing the developer token claims. </param>
public record DeveloperTokenPairDto(
    string ShortKey,
    string Jwt
);