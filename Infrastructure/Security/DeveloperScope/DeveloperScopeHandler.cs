using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Security.DeveloperScope;

/// <summary>
/// Authorization handler that verifies if the user possesses the required scope in a developer token.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Checks if there is an identity of the type "DeveloperToken" in the user context.</item>
/// <item>Retrieves all claims of type "scope" and compares them with the required scope.</item>
/// <item>Logs information about the required scope and the user's scopes.</item>
/// <item>Calls <see cref="AuthorizationHandlerContext.Succeed"/> if the required scope is present.</item>
/// </list>
/// </remarks>
public class DeveloperScopeHandler : AuthorizationHandler<DeveloperScopeRequirement>
{
    /// <summary>
    /// Handles the verification of the required scope for the user.
    /// </summary>
    /// <param name="context">Authorization context containing the user's identities.</param>
    /// <param name="requirement">Requirement specifying the required scope.</param>
    /// <returns>A task representing the completion of the handling process.</returns>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        DeveloperScopeRequirement requirement)
    {
        var devIdentity = context.User.Identities.FirstOrDefault(i => i.AuthenticationType == "DeveloperToken");
        if (devIdentity == null) return Task.CompletedTask;

        var scopes = devIdentity.FindAll("scope").Select(c => c.Value).ToList();
        
        if (requirement.RequiredScope != null && scopes.Contains(requirement.RequiredScope))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}