using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Host;

/// <summary>
/// Extension methods for automatic service discovery and registration.
/// </summary>
public static class ServiceDiscoveryExtensions
{
    /// <summary>
    /// Scans the given assemblies for classes matching the <see cref="ServiceDiscoveryOptions"/>
    /// and registers them into the DI container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="assemblies">Assemblies to scan for service types.</param>
    /// <param name="configure">Optional action to configure <see cref="ServiceDiscoveryOptions"/>.</param>
    /// <returns>The original <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// <list type="bullet">
    /// <item>Filters types based on allowed/excluded namespaces and types.</item>
    /// <item>Respects options like <see cref="ServiceDiscoveryOptions.SkipInterfaces"/> and <see cref="ServiceDiscoveryOptions.SkipExceptions"/>.</item>
    /// <item>Registers all discovered classes as their implemented interfaces with the specified lifetime.</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddDiscoveredServices(
        this IServiceCollection services,
        Assembly[] assemblies,
        Action<ServiceDiscoveryOptions>? configure = null)
    {
        var opts = new ServiceDiscoveryOptions();
        configure?.Invoke(opts);

        using var provider = services.BuildServiceProvider();
        var logger = provider.GetService<ILoggerFactory>()?.CreateLogger("ServiceDiscovery")
                     ?? NullLogger.Instance;

        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes
                .Where(type => ServiceDiscoveryFilter.IsValidServiceType(type, logger, opts)))
            .AsImplementedInterfaces()
            .WithLifetime(opts.Lifetime)
        );
        
        return services;
    }
}
