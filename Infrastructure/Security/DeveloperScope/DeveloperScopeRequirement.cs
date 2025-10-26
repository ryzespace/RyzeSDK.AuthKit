using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Security.DeveloperScope;

/// <summary>
/// Represents a developer token scope requirement for authorization.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Used by <see cref="DeveloperScopeHandler"/> to verify if the user has the required scope.</item>
/// <item>The <see cref="RequiredScope"/> property specifies the scope that must be present in the developer token.</item>
/// <item>Can be dynamically assigned in policies via <see cref="DeveloperScopePolicyProvider"/>.</item>
/// </list>
/// </remarks>
public class DeveloperScopeRequirement : IAuthorizationRequirement
{
    public string? RequiredScope { get; set; }
}