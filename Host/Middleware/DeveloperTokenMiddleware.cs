using System.Security.Claims;
using Application.Interfaces;

namespace Host.Middleware;

public class DeveloperTokenMiddleware(RequestDelegate next, ILogger<DeveloperTokenMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IDeveloperTokenValidator validator)
    {
        if (context.Request.Headers.TryGetValue("X-Developer-Token", out var tokenValue))
        {
            var rawToken = tokenValue.ToString().Trim();

            if (rawToken.StartsWith("\"") && rawToken.EndsWith("\""))
                rawToken = rawToken.Trim('"');

            if (rawToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                rawToken = rawToken.Substring("Bearer ".Length).Trim();

            logger.LogInformation("[AuthKit] Received X-Developer-Token: {Token}", rawToken);

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

                    logger.LogInformation("[AuthKit] Developer token validated for {DeveloperId}", principal.DeveloperId);
                    logger.LogInformation("[AuthKit] IsAuthenticated? {IsAuthenticated}", context.User.Identity?.IsAuthenticated);
                    logger.LogInformation("[AuthKit] IsInRole(User)? {IsInRole}", context.User.IsInRole("User"));
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
