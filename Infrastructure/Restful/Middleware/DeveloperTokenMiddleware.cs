using System.Security.Claims;
using Application.Features.DeveloperTokens.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Restful.Middleware;

/// <summary>
/// Middleware responsible for validating "DeveloperToken" headers and enriching <see cref="HttpContext.User"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Checks for the presence of the "X-Developer-Token" header.</item>
/// <item>Validates the token using <see cref="IDeveloperTokenValidator"/>.</item>
/// <item>Adds a <see cref="ClaimsIdentity"/> to <see cref="HttpContext.User"/> containing the developer ID, name, and scopes.</item>
/// <item>Logs validation success or failure for observability.</item>
/// <item>Does not short-circuit the request; the pipeline continues regardless of token validity.</item>
/// </list>
/// </remarks>
public class DeveloperTokenMiddleware(RequestDelegate next, ILogger<DeveloperTokenMiddleware> logger)
{
    /// <summary>
    /// Invokes the middleware logic to validate developer tokens and populate <see cref="HttpContext.User"/>.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="validator">Service used to validate developer tokens.</param>
    public async Task InvokeAsync(HttpContext context, IDeveloperTokenValidator validator)
    {
        if (context.Request.Headers.TryGetValue("X-Developer-Token", out var header))
        {
            var token = header.ToString().Trim().Trim('"');
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = token["Bearer ".Length..].Trim();

            try
            {
                var principal = await validator.ValidateAsync(token);
                if (principal != null)
                {
                    var identity = new ClaimsIdentity(
                        principal.Scopes.Select(s => new Claim("scope", s)),
                        "DeveloperToken"
                    );
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.DeveloperId.ToString()));
                    identity.AddClaim(new Claim(ClaimTypes.Name, principal.Name));
                    
                    context.User.AddIdentity(identity);

                    logger.LogDebug("[AuthKit] Developer token validated for {DeveloperId}", principal.DeveloperId);
                }

            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "[AuthKit] Failed to validate developer token.");
            }
        }

        await next(context);
    }
}