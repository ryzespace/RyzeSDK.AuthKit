using AuthKit.Plugins.Abstractions;

namespace PluginContractValidator.Rules;

/// <summary>
/// Ensures contributed OpenAPI security schemes are well-formed.
/// </summary>
/// <remarks>
/// The rule verifies that the dictionary returned by
/// <see cref="IAuthKitPlugin.GetSecuritySchemes"/> is not <c>null</c> and that each key
/// matches the <c>Name</c> of its
/// <see cref="AuthKit.Plugins.Abstractions.AuthKitSecuritySchemeDescriptor"/>.
/// </remarks>
public sealed class SecuritySchemesRule : IPluginContractRule
{
    /// <summary>Gets the rule name ("SecuritySchemes").</summary>
    public string Name => "SecuritySchemes";

    /// <summary>
    /// Validates the security schemes contributed by the plugin.
    /// </summary>
    /// <param name="plugin">The loaded plugin to validate.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the validation.</param>
    /// <returns>The list of security-scheme violations; empty when the schemes are valid.</returns>
    public Task<IReadOnlyList<string>> ValidateAsync(
        LoadedPlugin plugin,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        try
        {
            var schemes = plugin.Instance.GetSecuritySchemes();
            if (schemes is null)
            {
                errors.Add("security schemes: GetSecuritySchemes returned null");
                return Task.FromResult<IReadOnlyList<string>>(errors);
            }

            foreach (var pair in schemes)
            {
                if (pair.Value is null)
                {
                    errors.Add($"security schemes: scheme '{pair.Key}' has a null descriptor");
                }
                else if (pair.Key != pair.Value.Name)
                {
                    errors.Add($"security schemes: key '{pair.Key}' does not match descriptor Name '{pair.Value.Name}'");
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"security schemes: GetSecuritySchemes threw: {ex.Message}");
        }

        return Task.FromResult<IReadOnlyList<string>>(errors);
    }
}
