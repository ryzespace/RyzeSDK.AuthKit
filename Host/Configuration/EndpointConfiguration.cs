using System.Diagnostics;

namespace Host.Configuration;

/// <summary>
/// Provides extension methods to map application endpoints, including REST, gRPC, health checks, and metrics.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Maps root ("/") endpoint to a JSON object describing the service, version, environment, and available endpoints.</description></item>
/// <item><description>Maps controller routes via MapControllers.</description></item>
/// <item><description>Maps health check endpoint ("/health") for monitoring service availability.</description></item>
/// <item><description>Maps metrics endpoint ("/metrics") returning service uptime in seconds.</description></item>
/// </list>
/// </remarks>
public static class EndpointConfiguration
{
    /// <summary>
    /// Maps application-specific endpoints to the provided <see cref="WebApplication"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure endpoints for.</param>
    /// <returns>The configured <see cref="WebApplication"/> instance for method chaining.</returns>
    public static WebApplication MapAppEndpoints(this WebApplication app)
    {
        app.MapGet("/", () => Results.Json(new
        {
            name = "Auth Microservice",
            description = "Core service for authentication, authorization and identity management.",
            endpoints = new
            {
                rest = new
                {
                    baseUrl = "https://localhost:5000/api",
                    swagger = "https://localhost:5000/swagger"
                },
                grpc = new
                {
                    baseUrl = "https://localhost:5001",
                    note = "Use gRPC client to interact with this service."
                }
            },
            version = "1.0.0",
            environment = app.Environment.EnvironmentName
        }));
        app.MapControllers();

        app.MapGet("/health", () => Results.Ok(new { status = "Healthy", time = DateTime.UtcNow }))
            .WithName("HealthCheck")
            .WithTags("Monitoring");

        app.MapGet("/metrics", () => Results.Json(new { uptime = (DateTime.UtcNow - Process.GetCurrentProcess().StartTime).TotalSeconds }))
            .WithName("Metrics")
            .WithTags("Monitoring");
        
        return app;
    }
}