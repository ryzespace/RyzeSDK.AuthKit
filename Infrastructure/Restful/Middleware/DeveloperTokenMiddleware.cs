using System.Security.Claims;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Restful.Middleware;

/// <summary>
/// Middleware that extracts and validates a developer token from the X-Developer-Token header.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Reads the <c>X-Developer-Token</c> header from the incoming request.</item>
/// <item>Supports optional Bearer prefix and quoted strings in the token header.</item>
/// <item>Validates the token using <see cref="IDeveloperTokenValidator"/>.</item>
/// <item>If valid, adds a new <see cref="ClaimsIdentity"/> with scope claims to <see cref="HttpContext.User"/>.</item>
/// <item>Logs validation status, including developer ID and authentication state.</item>
/// <item>Passes the request to the next middleware in the pipeline regardless of validation outcome.</item>
/// </list>
/// </remarks>
public class DeveloperTokenMiddleware(RequestDelegate next, ILogger<DeveloperTokenMiddleware> logger)
{
    /// <summary>
    /// Invokes the middleware to validate the developer token in the request.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="validator">The developer token validator service.</param>
    /// <returns>A task representing the asynchronous middleware execution.</returns>
    public async Task InvokeAsync(HttpContext context, IDeveloperTokenValidator validator)
    {
        if (context.Request.Headers.TryGetValue("X-Developer-Token", out var tokenValue))
        {
            var rawToken = tokenValue.ToString().Trim();

            if (rawToken.StartsWith("\"") && rawToken.EndsWith("\""))
                rawToken = rawToken.Trim('"');

            if (rawToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                rawToken = rawToken.Substring("Bearer ".Length).Trim();

            try
            {
                var principal = await validator.ValidateAsync(rawToken);

                if (principal != null)
                {
                    var devIdentity = new ClaimsIdentity(
                        principal.Scopes.Select(s => new Claim("scope", s)),
                        "DeveloperToken"
                    );

                    devIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.DeveloperId.ToString()));
                    devIdentity.AddClaim(new Claim(ClaimTypes.Name, principal.Name));
                    
                    var currentUser = context.User;
                    context.User = new ClaimsPrincipal(currentUser.Identities.Concat([devIdentity]));

                    logger.LogDebug("[AuthKit] Developer token validated for {DeveloperId}", principal.DeveloperId);
                    logger.LogDebug("[AuthKit] IsAuthenticated? {IsAuthenticated}", context.User.Identity?.IsAuthenticated);
                    logger.LogDebug("[AuthKit] IsInRole(User)? {IsInRole}", context.User.IsInRole("User"));
                }


            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[AuthKit] Failed to validate developer token.");
            }
        }

        await next(context);
    }
}
