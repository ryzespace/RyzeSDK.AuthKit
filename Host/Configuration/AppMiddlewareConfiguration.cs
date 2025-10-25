using Host.Middleware;

namespace Host.Configuration;

/// <summary>
/// Provides extension methods to configure the application's middleware pipeline.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Configures routing, authentication, and authorization middleware.</description></item>
/// <item><description>Enables Swagger and SwaggerUI in development environment for API documentation.</description></item>
/// <item><description>Designed as an extension method to keep <see cref="Program.cs"/> minimal and clean.</description></item>
/// </list>
/// </remarks>
public static class AppMiddlewareConfiguration
{
    /// <summary>
    /// Configures routing, authentication, authorization, and Swagger (in development) for the application.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to configure.</param>
    /// <returns>The configured <see cref="WebApplication"/> instance for method chaining.</returns>
    public static WebApplication ConfigureMiddleware(this WebApplication app)
    {
        app.UseRouting();
        
        app.UseMiddleware<ValidationExceptionMiddleware>();
        app.UseMiddleware<DeveloperTokenMiddleware>();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        return app;
    }
}