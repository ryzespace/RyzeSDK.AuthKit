using Host.Configuration.Factory;
using JasperFx;
using Marten;
using Wolverine;
using Wolverine.FluentValidation;
using Wolverine.Marten;

namespace Host.Configuration;

/// <summary>
/// Provides infrastructure-level configuration for the application.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Registers Wolverine</item>
/// </list>
/// </remarks>
public static class InfrastructureConfiguration
{
    public static void ConfigureWolverine(
        this ConfigureHostBuilder hostBuilder,
        IConfiguration configuration)
    {
        hostBuilder.UseWolverine(opts =>
        {
            opts.UseFluentValidation();
            opts.IncludeEventHandlers();
        });
    }

    /// <summary>
    /// Configures Marten document store and integrates it with Wolverine.
    /// </summary>
    /// <param name="services">The service collection to add dependencies to.</param>
    /// <param name="configuration">Application configuration containing Marten connection strings.</param>
    public static IServiceCollection ConfigureMarten(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMarten(opts =>
            {
                opts.Connection(configuration.GetConnectionString("Marten")!);
                opts.AutoCreateSchemaObjects = AutoCreate.All;
            })
            .IntegrateWithWolverine()
            .UseLightweightSessions();

        services.AddScoped<IDocumentSession>(sp =>
            sp.GetRequiredService<IDocumentStore>().LightweightSession());

        return services;
    }
}