using System.Collections.Immutable;
using AuthKit.Plugins.Abstractions.Contracts;
using AuthKit.Plugins.Abstractions.Contracts.Plugins;

namespace AuthKit.Plugins.Abstractions.Models;

/// <summary>
/// Projection of plugin metadata available before activation.
/// This surface mirrors selected runtime members of <see cref="IAuthKitPlugin"/>.
/// </summary>
public sealed record PluginManifest
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string? Description { get; init; }
    public SemanticVersion Version { get; init; } = new(0,0,0);
    public string? Author { get; init; }
    public string? License { get; init; }
    public string? LicenseUrl { get; init; }
    public string? Homepage { get; init; }
    public string? RepositoryUrl { get; init; }

    /// <summary>
    /// Optional classification tags for UI filtering. Defaults to empty.
    /// Null or whitespace elements are invalid and should be rejected during validation.
    /// Used for filtering plugins in UIs and catalogs.
    /// </summary>
    /// <remarks>
    /// Tags are case-sensitive strings without controlled vocabulary.
    /// Example: ["security", "auth", "audit"].
    /// </remarks>
    public IReadOnlyList<string> Tags { get; init; } = [];
    /// <summary>
    /// Priority used for activation ordering among dependency-ready plugins.
    /// Lower values are activated earlier, higher values later.
    /// </summary>
    /// <remarks>
    /// The ordering algorithm is: topological sort where, among the set of currently
    /// dependency-ready plugins, the next plugin is chosen by Priority ascending.
    /// Dependency order (G7) wins over Priority.
    /// Example: A (p. 100) → B (p-100, DependsOn A), C (p. 0) ⇒ order: A, C, B.
    /// </remarks>
    public int Priority { get; init; }
    /// <summary>
    /// Indicates whether the plugin is enabled. Defaults to true.
    /// </summary>
    /// <remarks>
    /// If <c>false</c>, the plugin is skipped before loading (no consistency check runs for it).
    /// For plugins accepted by the preload gate and subsequently loaded,
    /// <c>manifest.IsEnabled == instance.IsEnabled</c> is part of consistency validation.
    /// </remarks>
    public bool IsEnabled { get; init; } = true;
    /// <summary>
    /// Features/capabilities exposed by the plugin. Contract
    /// requires Case insensitive comparison. Defaults to an empty set.
    /// </summary>
    /// <remarks>
    /// Used for pre-activation capability checks and post-load consistency validation.
    /// Host checks capabilities using the <see cref="PluginExtensions.Supports(AuthKit.Plugins.Abstractions.Models.PluginManifest,string)"/> extension method.
    /// Example: <c>manifest.Supports("auth")</c>.
    /// </remarks>
    public IReadOnlySet<string> Capabilities { get; init; } = ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Minimum host version required to load this plugin.
    /// </summary>
    /// <remarks>
    /// If the host version is lower than <see cref="MinHostVersion"/>, the plugin is rejected.
    /// Uses SemanticVersion 2.0.0 for version comparison.
    /// </remarks>
    public SemanticVersion? MinHostVersion { get; init; }

    /// <summary>
    /// List of plugin IDs this plugin depends on.
    /// </summary>
    /// <remarks>
    /// Each entry must be a valid plugin ID. The host validates:
    /// - No self-dependencies (plugin cannot depend on itself).
    /// - No duplicate dependencies.
    /// - All dependencies exist among discovered plugins.
    /// - No dependency cycles.
    /// </remarks>
    public IReadOnlyList<string> DependsOn { get; init; } = [];
}