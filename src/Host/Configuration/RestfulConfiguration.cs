using AuthKit.Plugins.Abstractions;
using Host.Plugins;
using Microsoft.OpenApi.Models;

namespace Host.Configuration;

/// <summary>
/// Provides extension methods for configuring AuthKit RESTful services and
/// OpenAPI/Swagger documentation.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Registers API explorer services required for endpoint metadata discovery.</item>
/// <item>Registers Swagger generation and configures the application's OpenAPI document.</item>
/// <item>Registers the built-in JWT bearer security scheme.</item>
/// <item>Registers additional security schemes contributed by loaded AuthKit plugins.</item>
/// <item>Converts AuthKit security scheme descriptors into OpenAPI security scheme definitions.</item>
/// </list>
/// </remarks>
public static class RestfulConfiguration
{
    /// <summary>
    /// Registers RESTful API services and configures Swagger/OpenAPI generation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="plugins">
    /// The plugins loaded by the Host. Each plugin may contribute one or more
    /// OpenAPI security scheme definitions.
    /// </param>
    /// <returns>The configured <see cref="IServiceCollection"/> instance.</returns>
    /// <remarks>
    /// <para>The method registers API explorer support and creates Swagger document named <c>v1</c>.</para>
    /// <para>
    /// A built-in HTTP Bearer authentication scheme is registered for JWT
    /// authentication. Every security scheme exposed by a loaded plugin is also
    /// added to the generated OpenAPI document together with a corresponding
    /// security requirement.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddRestfulServices(this IServiceCollection services, IReadOnlyList<LoadedPlugin> plugins)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            c.EnableAnnotations();

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insert JWT token in the format: Bearer {token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });

            foreach (var lp in plugins)
            {
                foreach (var (name, descriptor) in lp.Plugin.GetSecuritySchemes())
                {
                    c.AddSecurityDefinition(name, ToOpenApiSecurityScheme(descriptor));
                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = name }
                            },
                            Array.Empty<string>()
                        }
                    });
                }
            }
        });

        return services;
    }

    private static OpenApiSecurityScheme ToOpenApiSecurityScheme(AuthKitSecuritySchemeDescriptor descriptor) =>
        new()
        {
            Name = descriptor.Name,
            Type = descriptor.Type switch
            {
                AuthKitSecuritySchemeType.ApiKey => SecuritySchemeType.ApiKey,
                AuthKitSecuritySchemeType.Http => SecuritySchemeType.Http,
                AuthKitSecuritySchemeType.OAuth2 or AuthKitSecuritySchemeType.OpenIdConnect => SecuritySchemeType.OAuth2,
                AuthKitSecuritySchemeType.MutualTls => SecuritySchemeType.Http,
                AuthKitSecuritySchemeType.Session => SecuritySchemeType.Http,
                AuthKitSecuritySchemeType.Custom => SecuritySchemeType.Http,
                AuthKitSecuritySchemeType.Basic => SecuritySchemeType.Http,
                _ => SecuritySchemeType.Http
            },
            In = descriptor.In switch
            {
                AuthKitApiKeyLocation.Header => ParameterLocation.Header,
                AuthKitApiKeyLocation.Query => ParameterLocation.Query,
                AuthKitApiKeyLocation.Cookie => ParameterLocation.Cookie,
                _ => throw new ArgumentOutOfRangeException(nameof(descriptor))
            },
            Scheme = descriptor.Scheme,
            BearerFormat = descriptor.BearerFormat,
            Description = descriptor.Description
        };
}
