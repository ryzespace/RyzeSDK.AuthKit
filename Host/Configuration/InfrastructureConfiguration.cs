using Host.Configuration.Factory;
using Wolverine;
using Wolverine.FluentValidation;

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
}