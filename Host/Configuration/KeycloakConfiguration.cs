using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;

namespace Host.Configuration;

public static class KeycloakConfiguration
{
    public static IServiceCollection AddKeycloakServices(this IServiceCollection services)
    {
        var baseUrl = Environment.GetEnvironmentVariable("KEYCLOAK_URL") ?? "http://keycloak:8080";
        var realm = Environment.GetEnvironmentVariable("KEYCLOAK_REALM") ?? "authz";
        var clientId = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ID") ?? "workspace-authz";

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"{baseUrl}/realms/{realm}";
                options.Audience = clientId;
                options.RequireHttpsMetadata = false;
                
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers =
                    [
                        "http://localhost:8081/realms/authz",
                        "http://keycloak:8080/realms/authz"
                    ],
                    ValidateAudience = true,
                    ValidAudience = clientId,
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        if (ctx.Principal?.Identity is ClaimsIdentity identity)
                        {
                            var resourceAccessClaim = identity.FindFirst("resource_access")?.Value;
                            if (!string.IsNullOrEmpty(resourceAccessClaim))
                            {
                                var resourceAccess = JObject.Parse(resourceAccessClaim);
                                if (resourceAccess.TryGetValue("workspace-authz", out var workspaceClient))
                                {
                                    var roles = workspaceClient["roles"]?.ToObject<string[]>();
                                    if (roles != null)
                                    {
                                        var roleClaims = roles
                                            .Select(role => new Claim(ClaimTypes.Role, role))
                                            .ToList();
                                        identity.AddClaims(roleClaims);
                                    }
                                }
                            }
                        }
                        return Task.CompletedTask;
                    }
                };

                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            });

        return services;
    }
}
