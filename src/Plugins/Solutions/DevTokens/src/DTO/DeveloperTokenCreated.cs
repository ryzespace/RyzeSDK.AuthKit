namespace DevTokens.DTO;

/// <summary>
/// Represents the result of creating a developer token.
/// </summary>
/// <remarks>
/// <para>
/// Contains the credentials generated for the newly created developer token,
/// including its short API key and signed JWT.
/// </para>
/// <para>
/// Includes the <see cref="DeveloperToken"/> entity containing the persisted
/// token metadata and configuration.
/// </para>
/// </remarks>
public class DeveloperTokenCreated
{
    /// <summary>
    /// Gets or sets the short API key used as a public identifier
    /// for the developer token.
    /// </summary>
    /// <example>rk_live_xxx</example>
    public string ShortKey { get; set; } = null!;

    /// <summary>
    /// Gets or sets the signed JWT containing developer token claims
    /// and expiration information.
    /// </summary>
    public string Jwt { get; set; } = null!;

    /// <summary>
    /// Gets or sets the developer token entity associated with the
    /// generated credentials.
    /// </summary>
    public DeveloperToken Token { get; set; } = null!;
}