using Application.Features.DeveloperTokens.Interfaces;
using Application.Features.DeveloperTokens.Services;
using Application.Features.KeyManagement.Interfaces;
using Application.Features.KeyManagement.Services;
using Infrastructure.Repositories.KeyManagement;
using Infrastructure.Restful.Middleware.Exceptions;
using Infrastructure.Security;
using Infrastructure.Security.DeveloperScope;
using Microsoft.AspNetCore.Authorization;

namespace Host.Configuration;

public static class AuthKitConfiguration
{
    public static void AddAuthKitDeveloperToken(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        // Authorization pipeline
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationMiddlewareResultHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, DeveloperScopePolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, DeveloperScopeHandler>();
        
        services.AddScoped<IDeveloperTokenValidator, DeveloperTokenValidatorService>();
        services.AddScoped<IDeveloperTokenManager, DeveloperTokenManager>();
        
        services.AddSingleton<IKeyEncryptor>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var key = config["Encryption:AES_MASTER_KEY"]
                      ?? throw new InvalidOperationException("Missing AES_MASTER_KEY in configuration.");
            return new AesKeyEncryptor(key);
        });

        // RSA / JWT key stores
        services.AddSingleton<IKeyGenerator, RsaKeyGenerator>();
        services.AddScoped<IKeyStorePersistence, KeyStorePersistence>();
        services.AddScoped<IJwtKeyStore, JwtKeyStore>();
        services.AddHostedService<JwtKeyStoreInitializer>();
    }
}