using Microsoft.AspNetCore.Authorization;

namespace DevTokens.Security;

/// <summary>
/// Represents an authorization requirement for specific developer token scope.
/// </summary>
/// <remarks>
/// <para>
/// The requirement is evaluated by <see cref="DeveloperScopeHandler"/>, which
/// determines whether the current identity contains the required developer token scope.
/// </para>
/// <para>
/// The required scope is assigned dynamically by
/// <see cref="DeveloperScopePolicyProvider"/> when creating scope based authorization policies.
/// </para>
/// </remarks>
public class DeveloperScopeRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets or sets the scope that must be present in the developer token
    /// for authorization to succeed.
    /// </summary>
    public string? RequiredScope { get; set; }
}