using System.Reflection;
using Application.Options;
using Application.UseCase.Commands.Requests;
using Domain.ValueObject;
using FluentValidation;
using Infrastructure.Restful.Controllers;

namespace Host.Configuration;

/// <summary>
/// Initializes application services, logging, options, HTTP context, validators, and custom dependencies.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Registers logging configuration from <see cref="IConfiguration"/> and console logging.</description></item>
/// <item><description>Adds options and <see cref="IHttpContextAccessor"/> for application-wide access.</description></item>
/// <item><description>Registers controllers from host and infrastructure assemblies.</description></item>
/// <item><description>Scans assemblies to automatically register FluentValidation validators.</description></item>
/// <item><description>Registers custom dependency injection for application services, domain interfaces, and Keycloak infrastructure.</description></item>
/// </list>
/// </remarks>
public static class ApplicationInitialization
{
    /// <summary>
    /// Configures the application with logging, options, HTTP context, validators, and custom DI.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configuration">The application <see cref="IConfiguration"/> for settings.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> for method chaining.</returns>
    public static IServiceCollection ConfigureApp(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddLogging(logging =>
        {  
            logging.AddConsole();
            logging.AddConfiguration(configuration.GetSection("Logging"));
        });
        services.AddOptions();
        services.AddHttpContextAccessor();

        AddServices(services);
        services.Configure<AuthKitOptions>(
            configuration.GetSection("AuthKit"));
        return services;
    }

    #region Private Helpers

    private static void AddServices(this IServiceCollection services)
    {
        services.AddControllers()
                .AddApplicationPart(typeof(DeveloperTokensController).Assembly);

        services.AddFluentValidation()
                .AddCustomDependencyInjection();
    }

    private static Assembly[] GetRelevantAssemblies() =>
        new[]
        {
            typeof(ApplicationInitialization).Assembly,  // Host
            typeof(TokenName).Assembly,               // Domain
            typeof(DeveloperTokensController).Assembly,            // Infrastructure
            typeof(CreateDeveloperTokenCommand).Assembly        // Application
        }.Distinct().ToArray();

    private static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        var assemblies = GetRelevantAssemblies();
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableTo(typeof(AbstractValidator<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    private static void AddCustomDependencyInjection(this IServiceCollection services)
    {
        var assemblies = GetRelevantAssemblies();
        var namespacesToScan = new[]
        {
            "Application.Service",
            "Application.Interfaces",
            "Application.UseCase.Commands.Handlers",
            "Domain.Interfaces",
            "Infrastructure.Keycloak",
            "Infrastructure.Repositories"
        };

        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes
                .Where(type => namespacesToScan.Any(ns =>
                    type.Namespace?.StartsWith(ns) ?? false)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }

    #endregion
}