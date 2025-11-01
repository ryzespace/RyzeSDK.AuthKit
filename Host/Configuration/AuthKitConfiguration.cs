using Application.Interfaces;
using Infrastructure.Restful.Middleware;
using Infrastructure.Restful.Middleware.Exceptions;
using Infrastructure.Security;
using Infrastructure.Security.DeveloperScope;
using Infrastructure.Security.DeveloperToken;
using Microsoft.AspNetCore.Authorization;

namespace Host.Configuration;

public static class AuthKitConfiguration
{
    public static void AddAuthKitDeveloperToken(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();

        services.AddScoped<IDeveloperTokenValidator, DeveloperTokenValidator>();
        services.AddSingleton<IAuthorizationPolicyProvider, DeveloperScopePolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, DeveloperScopeHandler>();
    }
}