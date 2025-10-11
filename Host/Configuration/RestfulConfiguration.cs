using Infrastructure.Restful.Controllers;

namespace Host.Configuration;

/// <summary>
/// Provides extension methods to register RESTful services, controllers, and Swagger in the application.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Registers controllers from the <see cref="AuthController"/> assembly.</description></item>
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
        services.AddControllers()
            .AddApplicationPart(typeof(AuthController).Assembly);

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}