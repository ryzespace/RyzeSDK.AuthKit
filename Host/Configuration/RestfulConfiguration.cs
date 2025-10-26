using Infrastructure.Restful.Controllers;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

namespace Host.Configuration;

/// <summary>
/// Provides extension methods to register RESTful services, controllers, and Swagger in the application.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Registers controllers from the <see cref="SdkAuthController"/> assembly.</description></item>
/// <item><description>Adds API explorer support for endpoint metadata.</description></item>
/// <item><description>Registers Swagger generator for API documentation.</description></item>
/// </list>
/// </remarks>
public static class RestfulConfiguration
{
    /// <summary>
    /// Adds RESTful services and Swagger generation to the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> for method chaining.</returns>
    public static IServiceCollection AddRestfulServices(this IServiceCollection services)
    {
        services.AddControllers().AddApplicationPart(typeof(SdkAuthController).Assembly);
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insert JWT token in the format: Bearer {token}"
            });

            c.AddSecurityDefinition("X-Developer-Token", new OpenApiSecurityScheme
            {
                Name = "X-Developer-Token",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "AuthKit developer JWT token"
            });
            
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "X-Developer-Token" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}