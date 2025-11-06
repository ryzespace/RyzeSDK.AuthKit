using Application.Interfaces;
using Application.Interfaces.JWTKey;
using Application.Services;
using Application.Services.Key;
using Application.UseCase.Commands.Handlers;
using Infrastructure.Restful.Middleware.Exceptions;
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
        services.AddSingleton<IAuthorizationPolicyProvider, DeveloperScopePolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, DeveloperScopeHandler>();

        services.AddScoped<IDeveloperTokenValidator, DeveloperTokenValidator>();

        // AES encryptor
        services.AddSingleton<IKeyEncryptor>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var key = config["Encryption:AES_MASTER_KEY"]
                      ?? throw new InvalidOperationException("Missing AES_MASTER_KEY in configuration.");
            return new AesKeyEncryptor(key);
        });
        services.AddSingleton<IKeyGenerator, RsaKeyGenerator>();        
        services.AddSingleton<IKeyStorePersistence, FileKeyStorePersistence>();
        services.AddSingleton<IJwtKeyStore, JwtKeyStore>();

        // Developer token services
        services.AddScoped<IDeveloperTokenService, DeveloperTokenService>();
        services.AddScoped<IDeveloperTokenManager, DeveloperTokenManager>();
    }
}