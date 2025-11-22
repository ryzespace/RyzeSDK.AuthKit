using System.Reflection;
using Application.Features.DeveloperTokens.UseCase.Commands.Requests;
using Application.Features.KeyManagement.Services;
using Application.Options;
using Domain.Features.DeveloperTokens.ValueObject;
using FluentValidation;
using Infrastructure.Repositories.KeyManagement;
using Infrastructure.Restful.DeveloperTokens;

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
    /// <param name="logger"></param>
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

        services.AddControllers()
            .AddApplicationPart(typeof(DeveloperTokensController).Assembly);

        services.AddDiscoveredServices(GetRelevantAssemblies(), opts =>
        {
            opts.ExcludedTypes.Add(typeof(AesKeyEncryptor));
            opts.ExcludedTypes.Add(typeof(RsaKeyGenerator));
            opts.ExcludedTypes.Add(typeof(JwtKeyStore));
            opts.ExcludedTypes.Add(typeof(KeyStorePersistence));
        })
       .AddFluentValidation();
        
        services.ConfigureAppOptions(configuration);
        
        return services;
    }
    
    #region Private Options FluentValidation

    extension(IServiceCollection services)
    {
        private void ConfigureAppOptions(IConfiguration configuration)
        {
            services.Configure<AuthKitOptions>(configuration.GetSection("AuthKit"));
            services.Configure<ErrorMetadataOptions>(configuration.GetSection("ErrorMetadata"));
        }

        private void AddFluentValidation()
        {
            services.Scan(scan => scan
                .FromAssemblies(GetRelevantAssemblies())
                .AddClasses(classes => classes.AssignableTo(typeof(AbstractValidator<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        }
    }

    #endregion
    
    #region Private Helpers
    private static Assembly[] GetRelevantAssemblies() =>
        new[]
        {
            typeof(ApplicationInitialization).Assembly,  // Host
            typeof(TokenName).Assembly,               // Domain
            typeof(DeveloperTokensController).Assembly,            // Infrastructure
            typeof(CreateDeveloperTokenCommand).Assembly        // Application
        }.Distinct().ToArray();
    #endregion
}