namespace Application.Services;

/// <summary>
/// Holds configuration settings required to connect and authenticate with Keycloak.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Reads default values from environment variables if not explicitly set.</description></item>
/// <item><description>Used by providers and repositories to access the Keycloak server and realm.</description></item>
/// <item><description>Contains credentials for client authentication (client ID and secret).</description></item>
/// </list>
/// </remarks>
public class KeycloakSettings
{
    public string BaseUrl { get; init; } = Environment.GetEnvironmentVariable("KEYCLOAK_URL") ?? "";
    public string Realm { get; init; } = Environment.GetEnvironmentVariable("KEYCLOAK_REALM") ?? "";
    public string ClientId { get; init; } = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ID") ?? "";
    public string ClientSecret { get; init; } = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_SECRET") ?? "";
}
