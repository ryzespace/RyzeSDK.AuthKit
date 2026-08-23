using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Host.ServiceDiscovery;

/// <summary>
/// Provides extension methods for automatically discovering and registering
/// application services in the dependency injection container.
/// </summary>
/// <remarks>
/// <para>
/// Services are discovered by scanning the specified assemblies and filtering
/// types according to <see cref="ServiceDiscoveryOptions"/>.
/// </para>
/// <para>
/// Discovered concrete service types are registered against their implemented
/// interfaces using the configured service lifetime.
/// </para>
/// </remarks>
public static class ServiceDiscoveryExtensions
{
    /// <summary>
    /// Scans the specified assemblies for service types and registers the
    /// discovered services in the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to register discovered services into.</param>
    /// <param name="assemblies">The assemblies to scan for service implementations.</param>
    /// <param name="configure">An optional callback used to configure <see cref="ServiceDiscoveryOptions"/>.</param>
    /// <param name="logger">An optional logger used to report service discovery and filtering information.</param>
    /// <returns>The original <see cref="IServiceCollection"/> instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// Types are filtered using <see cref="ServiceDiscoveryFilter.IsValidServiceType"/>
    /// and the configured <see cref="ServiceDiscoveryOptions"/>.
    /// Interfaces and exception types can be excluded according to the configured
    /// discovery options.
    /// </para>
    /// <para>
    /// Valid concrete service types are registered as their implemented interfaces
    /// using the configured <see cref="ServiceLifetime"/>.
    /// </para>
    /// </remarks>
    public static IServiceCollection AddDiscoveredServices(
        this IServiceCollection services,
        Assembly[] assemblies,
        Action<ServiceDiscoveryOptions>? configure = null,
        ILogger? logger = null)
    {
        var opts = new ServiceDiscoveryOptions();
        configure?.Invoke(opts);

        logger ??= NullLogger.Instance;

        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes
                .Where(type =>
                    ServiceDiscoveryFilter.IsValidServiceType(
                        type,
                        logger,
                        opts)))
            .AsImplementedInterfaces()
            .WithLifetime(opts.Lifetime));

        return services;
    }
}
