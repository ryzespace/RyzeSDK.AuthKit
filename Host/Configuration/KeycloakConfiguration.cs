using Application.Services;
using NETCore.Keycloak.Client.Authentication;
using NETCore.Keycloak.Client.HttpClients.Implementation;

namespace Host.Configuration;

/// <summary>
/// Provides extension methods to configure Keycloak services and authentication in the DI container.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Registers <see cref="KeycloakSettings"/> and <see cref="KeycloakClient"/> as singletons.</description></item>
/// <item><description>Adds HTTP client support required by KeycloakClient.</description></item>
/// <item><description>Configures Keycloak authentication using the JWT Bearer scheme with provided realm and client credentials.</description></item>
/// <item><description>Outputs configuration info to console for quick verification.</description></item>
/// </list>
/// </remarks>
public static class KeycloakConfiguration
{
    /// <summary>
    /// Adds Keycloak services, client, and authentication to the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> for method chaining.</returns>
    public static IServiceCollection AddKeycloakServices(this IServiceCollection services)
    {
        var settings = new KeycloakSettings
        {
            BaseUrl = Environment.GetEnvironmentVariable("KEYCLOAK_URL")!,
            Realm = Environment.GetEnvironmentVariable("KEYCLOAK_REALM")!,
            ClientId = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ID")!,
            ClientSecret = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_SECRET")!
        };

        services.AddSingleton(settings);
        services.AddHttpClient();
        services.AddSingleton(_ => new KeycloakClient(settings.BaseUrl));

        services.AddKeycloakAuthentication(
            authenticationScheme: "Bearer",
            keycloakConfig: options =>
            {
                options.Url = settings.BaseUrl;
                options.Realm = settings.Realm;
                options.Issuer = $"{settings.BaseUrl}/realms/{settings.Realm}";
                options.Resource = settings.ClientId;
                options.RequireSsl = false;
            },
            configureOptions: opts => opts.SaveToken = true);

        Console.WriteLine($"✅ Keycloak configured at: {settings.BaseUrl}/realms/{settings.Realm}");
        return services;
    }
}
