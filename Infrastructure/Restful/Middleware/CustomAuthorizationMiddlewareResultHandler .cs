using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Restful.Middleware;

/// <summary>
/// Custom authorization middleware result handler that returns JSON responses for 401 and 403 statuses.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Handles unauthorized (401) requests with a JSON payload containing an error message.</item>
/// <item>Handles forbidden (403) requests with a JSON payload containing an error message and logs the user.</item>
/// <item>Delegates to the default <see cref="AuthorizationMiddlewareResultHandler"/> for all other cases.</item>
/// <item>Logs all unauthorized and forbidden requests using the injected <see cref="ILogger"/>.</item>
/// </list>
/// </remarks>
public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
    
    /// <inheritdoc />
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        var logger = context.RequestServices
            .GetRequiredService<ILogger<CustomAuthorizationMiddlewareResultHandler>>();

        if (authorizeResult.Challenged)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var payload401 = new
            {
                error = "unauthorized",
                message = "Authentication required to access this resource."
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload401, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
            return;
        }

        if (authorizeResult.Forbidden)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var payload403 = new
            {
                error = "forbidden",
                message = "You do not have permission to access this resource."
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload403, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
            return;
        }
        
        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
