using Host.Services;

namespace Host.Configuration;

/// <summary>
/// Provides extension methods to configure gRPC services and map gRPC endpoints in the application.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Registers gRPC services in the DI container.</description></item>
/// <item><description>Supports adding global interceptors for exception handling or logging (commented placeholder included).</description></item>
/// <item><description>Maps gRPC service endpoints to the application's request pipeline.</description></item>
/// </list>
/// </remarks>
public static class GrpcConfiguration
{
    /// <summary>
    /// Adds gRPC services to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> for method chaining.</returns>
    public static IServiceCollection AddGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            //options.Interceptors.Add<ExceptionHandlingInterceptor>();
        });

        return services;
    }

    /// <summary>
    /// Maps gRPC service endpoints to the application's request pipeline.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The configured <see cref="WebApplication"/> for method chaining.</returns>
    public static WebApplication MapGrpcEndpoints(this WebApplication app)
    {
        app.MapGrpcService<GreeterService>();
        return app;
    }
}