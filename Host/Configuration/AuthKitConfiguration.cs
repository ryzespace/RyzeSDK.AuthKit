using Application.Interfaces;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;

namespace Host.Configuration;

public static class AuthKitConfiguration
{
    public static void AddAuthKitDeveloperToken(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        // AuthKit services
        services.AddScoped<IDeveloperTokenValidator, DeveloperTokenValidator>();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("DeveloperScope:createvm",
                policy => policy.Requirements.Add(new DeveloperScopeRequirement("createvm")));
            options.AddPolicy("DeveloperScope:deletevm",
                policy => policy.Requirements.Add(new DeveloperScopeRequirement("deletevm")));
        });
        services.AddSingleton<IAuthorizationHandler, DeveloperScopeHandler>();
    }
}