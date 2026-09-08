using System.Reflection;
using System.Runtime.Loader;
using AuthKit.Plugins.Abstractions.Contracts;
using AuthKit.Plugins.Abstractions.Models;

namespace Host.Plugins;

/// <summary>
/// Discovers and loads AuthKit plugins from a specified directory during host startup.
/// </summary>
/// <remarks>
/// <para>
/// Plugins are loaded before <see cref="Microsoft.AspNetCore.Builder.WebApplicationBuilder.Build"/>
/// is called, allowing infrastructure such as Wolverine, Marten, and MVC to discover
/// plugin assemblies while their configuration is being built.
/// </para>
/// <para>
/// Plugins are loaded into <see cref="AssemblyLoadContext.Default"/> rather than an
/// isolated load context. This ensures that shared framework and package types, such
/// as Wolverine <c>IMessageBus</c>, Marten <c>IDocumentSession</c>, and ASP.NET Core
/// MVC types, resolve to the same runtime types on both sides of the plugin boundary.
/// </para>
/// <para>
/// The loader does not support hot unloading or hot swapping of plugins. Plugins are
/// expected to remain loaded for the lifetime of the host process.
/// </para>
/// </remarks>
public static class PluginLoader
{
    /// <summary>
    /// Discovers and loads all valid AuthKit plugins from the specified root directory.
    /// </summary>
    /// <param name="pluginsRootPath">The root directory containing one subdirectory per plugin.</param>
    /// <param name="logger">The logger used to report plugin discovery, loading, and validation results.</param>
    /// <param name="hostVersion">The version of the host application.</param>
    /// <returns>A readonly collection containing all successfully loaded plugins.</returns>
    /// <remarks>
    /// Each plugin directory is expected to contain an entry assembly whose file name
    /// matches the directory name. Directories without matching assembly or assemblies
    /// without valid <see cref="IAuthKitPlugin"/> implementation are skipped.
    /// Plugins requiring a higher host version than <paramref name="hostVersion"/> are rejected.
    /// </remarks>
    public static IReadOnlyList<LoadedPlugin> LoadPlugins(string pluginsRootPath, ILogger logger, SemanticVersion hostVersion)
    {
        if (!Directory.Exists(pluginsRootPath))
        {
            logger.LogWarning("Plugins path '{Path}' does not exist — starting with zero plugins.", pluginsRootPath);
            return [];
        }

        var loaded = new List<LoadedPlugin>();

        foreach (var pluginDir in Directory.GetDirectories(pluginsRootPath))
        {
            if (TryLoadPlugin(pluginDir, logger, hostVersion, loaded, out var plugin))
            {
                loaded.Add(plugin);
                logger.LogInformation("Loaded plugin '{Name}' v{Version} from {Dir}", plugin.Plugin.Name, plugin.Plugin.Version.ToString(), pluginDir);
            }
        }

        return loaded;
    }

    private static bool TryLoadPlugin(
        string pluginDir,
        ILogger logger,
        SemanticVersion hostVersion,
        IReadOnlyCollection<LoadedPlugin> alreadyLoaded,
        out LoadedPlugin loadedPlugin)
    {
        loadedPlugin = null!;

        var pluginName = Path.GetFileName(pluginDir);
        var entryDllPath = Path.Combine(pluginDir, $"{pluginName}.dll");

        if (!File.Exists(entryDllPath))
        {
            logger.LogError(
                "Skipping plugin folder '{Dir}': expected entry assembly '{Dll}' not found.",
                pluginDir, entryDllPath);
            return false;
        }

        try
        {
            var manifest = TryReadManifest(pluginDir, pluginName, logger);

            if (manifest is not null && !ManifestIsUsable(manifest, pluginDir, alreadyLoaded, logger))
                return false;

            var assembly = LoadAssembly(entryDllPath);
            var pluginType = FindPluginType(assembly);

            if (pluginType is null)
            {
                logger.LogError(
                    "Skipping plugin assembly '{Dll}': no public, non-abstract IAuthKitPlugin implementation with a parameterless constructor found.",
                    entryDllPath);
                return false;
            }

            var plugin = (IAuthKitPlugin)Activator.CreateInstance(pluginType)!;

            if (!string.IsNullOrWhiteSpace(plugin.Id) &&
                alreadyLoaded.Any(lp => string.Equals(lp.Plugin.Id, plugin.Id, StringComparison.Ordinal)))
            {
                logger.LogError("Skipping plugin '{Dir}': duplicate plugin Id '{Id}' detected.", pluginDir, plugin.Id);
                return false;
            }

            // Without manifest, MinHostVersion (if declared on the instance) is the only
            // compatibility check available. With a manifest, this is covered by ValidateConsistency below.
            if (manifest is null && plugin.MinHostVersion is not null && hostVersion < plugin.MinHostVersion)
            {
                logger.LogError(
                    "Skipping plugin '{Dir}': host version {HostVersion} is lower than required minimum {MinVersion}.",
                    pluginDir, hostVersion.ToString(), plugin.MinHostVersion.ToString());
                return false;
            }

            if (manifest is not null)
            {
                try
                {
                    PluginValidator.ValidateConsistency(manifest, plugin);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Skipping plugin '{Dir}': manifest/instance consistency check failed.", pluginDir);
                    return false;
                }
            }

            loadedPlugin = new LoadedPlugin(plugin, assembly, pluginDir);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load plugin from '{Dir}'.", pluginDir);
            return false;
        }
    }

    private static PluginManifest? TryReadManifest(string pluginDir, string pluginName, ILogger logger)
    {
        string[] manifestCandidates =
        [
            Path.Combine(pluginDir, "manifest.json"),
            Path.Combine(pluginDir, $"{pluginName}.manifest.json")
        ];

        var manifestPath = manifestCandidates.FirstOrDefault(File.Exists);
        if (manifestPath is null)
            return null;

        try
        {
            var json = File.ReadAllText(manifestPath);
            return System.Text.Json.JsonSerializer.Deserialize<PluginManifest>(
                json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse manifest {Manifest} for plugin {Dir}", manifestPath, pluginDir);
            return null;
        }
    }

    private static bool ManifestIsUsable(
        PluginManifest manifest,
        string pluginDir,
        IReadOnlyCollection<LoadedPlugin> alreadyLoaded,
        ILogger logger)
    {
        try
        {
            PluginValidator.ValidateTags(manifest.Tags);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Skipping plugin '{Dir}': invalid tags in manifest.", pluginDir);
            return false;
        }

        try
        {
            PluginValidator.ValidateDependsOn(manifest.DependsOn, manifest.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Skipping plugin '{Dir}': invalid dependencies in manifest.", pluginDir);
            return false;
        }

        if (!manifest.IsEnabled)
        {
            logger.LogInformation("Skipping plugin '{Dir}': manifest indicates IsEnabled=false.", pluginDir);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(manifest.Id) &&
            alreadyLoaded.Any(lp => string.Equals(lp.Plugin.Id, manifest.Id, StringComparison.Ordinal)))
        {
            logger.LogError("Skipping plugin '{Dir}': duplicate plugin Id '{Id}' detected in manifest.", pluginDir, manifest.Id);
            return false;
        }

        return true;
    }

    private static Assembly LoadAssembly(string entryDllPath)
    {
        var resolver = new AssemblyDependencyResolver(entryDllPath);
        Func<AssemblyLoadContext, AssemblyName, Assembly?> handler = (context, name) =>
        {
            var path = resolver.ResolveAssemblyToPath(name);
            return path is not null ? context.LoadFromAssemblyPath(path) : null;
        };

        AssemblyLoadContext.Default.Resolving += handler;
        try
        {
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(entryDllPath);
        }
        finally
        {
            AssemblyLoadContext.Default.Resolving -= handler;
        }
    }

    private static Type? FindPluginType(Assembly assembly) =>
        assembly.GetTypes().FirstOrDefault(t =>
            t is { IsPublic: true, IsAbstract: false } &&
            typeof(IAuthKitPlugin).IsAssignableFrom(t) &&
            t.GetConstructor(Type.EmptyTypes) is not null);
}