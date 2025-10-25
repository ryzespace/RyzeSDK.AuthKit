using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Security;

public class DeveloperScopeRequirement(string scope) : IAuthorizationRequirement
{
    public string Scope { get; } = scope;
}
