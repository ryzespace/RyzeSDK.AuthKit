using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Security.DeveloperScope;

/// <summary>
/// Custom <see cref="IAuthorizationPolicyProvider"/> that dynamically creates policies
/// based on developer token scopes.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>If the policy name starts with "DeveloperScope:", extracts the scope and creates
/// a policy with <see cref="DeveloperScopeRequirement"/>.</item>
/// <item>Otherwise, falls back to the default <see cref="DefaultAuthorizationPolicyProvider"/>.</item>
/// <item>Supports retrieval of default and fallback policies via the underlying provider.</item>
/// </list>
/// </remarks>
public class DeveloperScopePolicyProvider(IServiceProvider services) : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider = new(
        services.GetRequiredService<IOptions<AuthorizationOptions>>());

    /// <inheritdoc />
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith("DeveloperScope:")) return _fallbackPolicyProvider.GetPolicyAsync(policyName);
        var scope = policyName.Split(':')[1];

        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new DeveloperScopeRequirement { RequiredScope = scope })
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);

    }
    /// <inheritdoc />
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();
    
    /// <inheritdoc />
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();
}