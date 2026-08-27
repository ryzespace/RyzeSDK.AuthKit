using System.Text.RegularExpressions;
using AuthKit.Plugins.Abstractions;

namespace PluginContractValidator.Rules;

/// <summary>
/// Ensures plugin metadata (Name, Version) is present and well-formed.
/// </summary>
/// <remarks>
/// The rule verifies that <see cref="IAuthKitPlugin.Name"/> is non-empty and that
/// <see cref="IAuthKitPlugin.Version"/> follows the SemVer
/// (<c>major.minor.patch[-prerelease]</c>) format expected by the host.
/// </remarks>
public sealed class MetadataRule : IPluginContractRule
{
    private static readonly Regex SemVer =
        new(@"^\d+\.\d+\.\d+(-[0-9A-Za-z-.]+)?$", RegexOptions.Compiled);

    /// <summary>Gets the rule name ("Metadata").</summary>
    public string Name => "Metadata";

    /// <summary>
    /// Validates the plugin's metadata.
    /// </summary>
    /// <param name="plugin">The loaded plugin to validate.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the validation.</param>
    /// <returns>The list of metadata violations; empty when the metadata is valid.</returns>
    public Task<IReadOnlyList<string>> ValidateAsync(
        LoadedPlugin plugin,
        CancellationToken cancellationToken = default)
    {
        var pluginInstance = plugin.Instance;
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(pluginInstance.Name))
        {
            errors.Add("metadata: Name is null or empty");
        }

        if (string.IsNullOrWhiteSpace(pluginInstance.Version))
        {
            errors.Add("metadata: Version is null or empty");
        }
        else if (!SemVer.IsMatch(pluginInstance.Version))
        {
            errors.Add($"metadata: Version '{pluginInstance.Version}' is not SemVer (expected major.minor.patch[-prerelease])");
        }

        return Task.FromResult<IReadOnlyList<string>>(errors);
    }
}
