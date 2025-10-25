using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Security;

public class DeveloperScopeHandler(ILogger<DeveloperScopeHandler> logger)
    : AuthorizationHandler<DeveloperScopeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        DeveloperScopeRequirement requirement)
    {
        var devIdentity = context.User.Identities
            .FirstOrDefault(i => i.AuthenticationType == "DeveloperToken");

        if (devIdentity == null)
        {
            logger.LogWarning("DeveloperScopeHandler: no DeveloperToken identity found");
            return Task.CompletedTask;
        }

        var scopeClaims = devIdentity.FindAll("scope").Select(c => c.Value).ToList();

        logger.LogInformation("DeveloperScopeHandler: scopes in user: {Scopes}", string.Join(", ", scopeClaims));

        if (scopeClaims.Contains(requirement.Scope))
        {
            context.Succeed(requirement);
            logger.LogInformation("DeveloperScopeHandler: requirement succeeded for scope {RequiredScope}", requirement.Scope);
        }
        else
        {
            logger.LogWarning("DeveloperScopeHandler: requirement NOT met for scope {RequiredScope}", requirement.Scope);
        }


        return Task.CompletedTask;
    }
}