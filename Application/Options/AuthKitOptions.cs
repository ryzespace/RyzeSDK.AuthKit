namespace Application.Options;

/// <summary>
/// Configuration options for the AuthKit module.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Defines settings related to developer token management.</item>
/// <item>Can be bound from appsettings.json or environment variables.</item>
/// </list>
/// </remarks>
public class AuthKitOptions
{
    public int MaxDeveloperTokens { get; set; } = 3;
}