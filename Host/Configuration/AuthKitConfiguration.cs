using Application.Interfaces;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;

namespace Host.Configuration;

public static class AuthKitConfiguration
{
    public static void AddAuthKitDeveloperToken(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        
        services.AddScoped<IDeveloperTokenValidator, DeveloperTokenValidator>();
        services.AddSingleton<IAuthorizationPolicyProvider, DeveloperScopePolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, DeveloperScopeHandler>();
    }
}