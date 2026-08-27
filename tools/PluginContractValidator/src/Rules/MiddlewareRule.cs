using AuthKit.Plugins.Abstractions;
using Microsoft.AspNetCore.Http;

namespace PluginContractValidator.Rules;

/// <summary>
/// Ensures a contributed middleware type follows the ASP.NET Core middleware convention.
/// </summary>
/// <remarks>
/// The middleware type must expose a constructor accepting
/// <see cref="RequestDelegate"/> and an <c>InvokeAsync(HttpContext, ...)</c> method that returns
/// <see cref="Task"/>, mirroring the conventional ASP.NET Core middleware shape.
/// </remarks>
public sealed class MiddlewareRule : IPluginContractRule
{
    /// <summary>Gets the rule name ("Middleware").</summary>
    public string Name => "Middleware";

    /// <summary>
    /// Validates the middleware type contributed by <see cref="IAuthKitPlugin.MiddlewareType"/>,
    /// if any.
    /// </summary>
    /// <param name="plugin">The loaded plugin to validate.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the validation.</param>
    /// <returns>
    /// The list of middleware violations; empty when the middleware is valid or absent.
    /// </returns>
    public Task<IReadOnlyList<string>> ValidateAsync(
        LoadedPlugin plugin,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var middlewareType = plugin.Instance.MiddlewareType;

        if (middlewareType is null)
        {
            return Task.FromResult<IReadOnlyList<string>>(errors);
        }

        var hasRequestDelegateCtor = middlewareType.GetConstructors()
            .Any(c => c.GetParameters().Any(p => p.ParameterType == typeof(RequestDelegate)));

        if (!hasRequestDelegateCtor)
        {
            errors.Add($"middleware: MiddlewareType '{middlewareType.Name}' does not have a constructor accepting RequestDelegate");
        }

        var invoke = middlewareType.GetMethod("InvokeAsync");
        if (invoke is null || invoke.ReturnType != typeof(Task))
        {
            errors.Add($"middleware: MiddlewareType '{middlewareType.Name}' must expose InvokeAsync returning Task");
        }
        else
        {
            var parameters = invoke.GetParameters();
            if (parameters.Length == 0 || parameters[0].ParameterType != typeof(HttpContext))
            {
                errors.Add($"middleware: InvokeAsync on '{middlewareType.Name}' must take HttpContext as its first parameter");
            }
        }

        return Task.FromResult<IReadOnlyList<string>>(errors);
    }
}
