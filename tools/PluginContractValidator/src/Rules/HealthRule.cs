using AuthKit.Plugins.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PluginContractValidator.Rules;

/// <summary>
/// Ensures <c>CheckHealthAsync</c> does not throw when invoked against a built service
/// provider (the host treats a throwing health check as a contract violation).
/// </summary>
public sealed class HealthRule : IPluginContractRule
{
    /// <summary>Gets the rule name ("Health").</summary>
    public string Name => "Health";

    /// <summary>
    /// Builds a service provider from the plugin's registered services and invokes
    /// <see cref="IAuthKitPlugin.CheckHealthAsync"/>, capturing any thrown exception as a violation.
    /// </summary>
    /// <param name="plugin">The loaded plugin to validate.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the validation.</param>
    /// <returns>The list of health check violations; empty when the check completes without throwing.</returns>
    public async Task<IReadOnlyList<string>> ValidateAsync(
        LoadedPlugin plugin,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        plugin.Instance.ConfigureServices(services, configuration);

        try
        {
            await using var provider = services.BuildServiceProvider();
            await plugin.Instance.CheckHealthAsync(provider);
        }
        catch (Exception ex)
        {
            errors.Add($"health: CheckHealthAsync threw (must not throw): {ex.Message}");
        }

        return errors;
    }
}
