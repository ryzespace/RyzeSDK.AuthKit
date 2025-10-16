using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Host.Configuration;

public static class KeycloakConfiguration
{
    /// <summary>
    /// Adds Keycloak authentication to the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The configured <see cref="IServiceCollection"/> for method chaining.</returns>
    public static IServiceCollection AddKeycloakServices(this IServiceCollection services)
    {
        var settings = new KeycloakSettings
        {
            BaseUrl = Environment.GetEnvironmentVariable("KEYCLOAK_URL")!,
            Realm = Environment.GetEnvironmentVariable("KEYCLOAK_REALM")!,
            ClientId = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ID")!,
            ClientSecret = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_SECRET")!
        };

        services.AddSingleton(settings);
        services.AddHttpClient();
       
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"{settings.BaseUrl}/realms/{settings.Realm}";
                options.Audience = "workspace-authz";
                options.RequireHttpsMetadata = false;
               
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true, 
                    ValidAudience = "workspace-authz",
                    ValidIssuer = $"http://localhost:8081/realms/{settings.Realm}",
                    ValidateLifetime = true,
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role,
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        if (ctx.SecurityToken is JwtSecurityToken jwt)
                        {
                            var identity = ctx.Principal!.Identity as ClaimsIdentity;

                            // Realm roles
                            var realmRoles = jwt.Claims.FirstOrDefault(c => c.Type == "realm_access")?.Value;
                            if (realmRoles != null)
                            {
                                var roles = JObject.Parse(realmRoles)["roles"]?.ToObject<string[]>();
                                if (roles != null)
                                {
                                    foreach (var role in roles)
                                        identity!.AddClaim(new Claim(ClaimTypes.Role, role));
                                }
                            }

                            // Resource roles (np. account)
                            var resourceRoles = jwt.Claims.FirstOrDefault(c => c.Type == "resource_access")?.Value;
                            if (resourceRoles != null)
                            {
                                var roles = JObject.Parse(resourceRoles)["account"]?["roles"]?.ToObject<string[]>();
                                if (roles != null)
                                {
                                    foreach (var role in roles)
                                        identity!.AddClaim(new Claim(ClaimTypes.Role, role));
                                }
                            }
                            
                            var resourceAccess = JObject.Parse(jwt.Claims
                                .FirstOrDefault(c => c.Type == "resource_access")?.Value ?? "{}");

                            if (resourceAccess["workspace-authz"]?["roles"] is JArray roless)
                            {
                                foreach (var role in roless)
                                    identity!.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
                            }

                        }

                        return Task.CompletedTask;
                    }
                };
            });
            

        Console.WriteLine($"✅ Keycloak configured at: {settings.BaseUrl}/realms/{settings.Realm}");
        return services;
    }
}
