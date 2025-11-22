namespace Host;

/// <summary>
/// Filters types during service discovery for automatic DI registration.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Detects type category based on namespace segments (e.g., Services, Managers, Repositories, DTO, Entity, ValueObject, Validators, Middleware, Controller).</item>
/// <item>Detects the feature area from namespaces containing ".Features.{FeatureName}".</item>
/// <item>Respects <see cref="ServiceDiscoveryOptions"/> rules such as skipping interfaces, exceptions, excluded types or namespaces, and allowed namespaces.</item>
/// <item>Logs debug messages showing inclusion/skipping decision, detected type, feature, and full type name.</item>
/// </list>
/// </remarks>
public static class ServiceDiscoveryFilter
{
    private static readonly (string Key, string TypeName)[] NamespaceTypes =
    [
        (".DTO", "DTO"),
        (".Entity", "Entity"),
        (".ValueObject", "ValueObject"),
        (".Services", "Service"),
        (".Managers", "Manager"),
        (".Repositories", "Repository"),
        (".Validators", "Validator"),
        (".Middleware", "Middleware"),
        (".Controller", "Controller")
    ];

    /// <summary>
    /// Determines if a type is valid for automatic DI registration.
    /// </summary>
    /// <param name="type">Type to evaluate.</param>
    /// <param name="logger">Logger for debug output.</param>
    /// <param name="opts">Service discovery options controlling inclusion/exclusion rules.</param>
    /// <returns><c>true</c> if the type should be included; otherwise <c>false</c>.</returns>
    public static bool IsValidServiceType(Type type, ILogger logger, ServiceDiscoveryOptions opts)
    {
        var ns = type.Namespace ?? string.Empty;

        var detectedType = "Allowed";
        foreach (var (key, typeName) in NamespaceTypes)
        {
            if (!ns.Contains(key)) continue;
            detectedType = typeName;
            break;
        }

        var feature = GetFeature(ns);
        var include = ShouldInclude(type, ns, opts);

        logger.LogDebug("{Decision} Type({TypeKind} | Feature: {Feature}): {Type}",
            include ? "Including" : "Skipping",
            detectedType,
            feature,
            type.FullName);

        return include;
    }

    /// <summary>
    /// Extracts feature name from a namespace containing ".Features.{FeatureName}".
    /// </summary>
    /// <param name="ns">Namespace string.</param>
    /// <returns>Feature name or "Global" if none found.</returns>
    private static string GetFeature(string ns)
    {
        const string segment = ".Features.";
        var idx = ns.IndexOf(segment, StringComparison.Ordinal);
        if (idx < 0) return "Global";

        var start = idx + segment.Length;
        var end = ns.IndexOf('.', start);
        return end > 0 ? ns[start..end] : ns[start..];
    }

    /// <summary>
    /// Applies <see cref="ServiceDiscoveryOptions"/> rules to decide inclusion.
    /// </summary>
    private static bool ShouldInclude(Type type, string ns, ServiceDiscoveryOptions opts)
        => !(opts.SkipInterfaces && type.IsInterface)
           && !(opts.SkipExceptions && typeof(Exception).IsAssignableFrom(type))
           && !opts.ExcludedTypes.Contains(type)
           && !opts.ExcludedNamespaces.Any(ns.Contains)
           && (opts.AllowedNamespaces.Count == 0 || opts.AllowedNamespaces.Any(ns.Contains));
}