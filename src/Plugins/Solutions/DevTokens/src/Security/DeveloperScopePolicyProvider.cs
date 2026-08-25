using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DevTokens.Security;

/// <summary>
/// Provides authorization policies dynamically based on developer token scopes.
/// </summary>
/// <remarks>
/// <para>
/// Policies whose names start with <c>DeveloperScope:</c> are generated dynamically
/// and require the corresponding developer token scope.
/// </para>
/// <list type="bullet">
/// <item>Extracts the required scope from policy names using the <c>DeveloperScope:</c> prefix.</item>
/// <item>Creates an <see cref="AuthorizationPolicy"/> containing <see cref="DeveloperScopeRequirement"/> for the requested scope.</item>
/// <item>Delegates unknown policy names to the default <see cref="DefaultAuthorizationPolicyProvider"/>.</item>
/// <item>Delegates default and fallback policy resolution to the underlying provider.</item>
/// </list>
/// </remarks>
public class DeveloperScopePolicyProvider(
    IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider =
        new(options);

    /// <summary>
    /// Retrieves an authorization policy by name.
    /// </summary>
    /// <param name="policyName">The name of the authorization policy to retrieve.</param>
    /// <returns>
    /// A task containing the resolved <see cref="AuthorizationPolicy"/>,
    /// or <c>null</c> when no matching policy exists.
    /// </returns>
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith("DeveloperScope:"))
            return _fallbackPolicyProvider.GetPolicyAsync(policyName);

        var scope = policyName.Split(':')[1];

        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new DeveloperScopeRequirement
            {
                RequiredScope = scope
            })
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }

    /// <summary>
    /// Retrieves the application default authorization policy.
    /// </summary>
    /// <returns>A task containing the default <see cref="AuthorizationPolicy"/>.</returns>
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    /// <summary>
    /// Retrieves the application's fallback authorization policy.
    /// </summary>
    /// <returns>
    /// A task containing the fallback <see cref="AuthorizationPolicy"/>,
    /// or <c>null</c> when no fallback policy is configured.
    /// </returns>
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallbackPolicyProvider.GetFallbackPolicyAsync();
}
