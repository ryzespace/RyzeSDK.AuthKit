using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Restful.Middleware;

/// <summary>
/// Handles authorization results and returns standardized RFC 7807 ProblemDetails responses for 401 and 403 errors.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Returns consistent JSON ProblemDetails for unauthorized and forbidden responses.</item>
/// <item>Leverages <see cref="ErrorMetadataOptions"/> for dynamic error documentation URLs.</item>
/// <item>Logs security-related access issues for audit purposes.</item>
/// </list>
/// </remarks>
public sealed class CustomAuthorizationMiddlewareResultHandler(
    IOptions<ErrorMetadataOptions> options) : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
    private readonly string _baseUrl = options.Value.DocsBaseUrl.TrimEnd('/');

    /// <inheritdoc />
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            await WriteProblemAsync(context,
                status: HttpStatusCode.Unauthorized,
                code: "unauthorized",
                title: "Unauthorized",
                detail: "Authentication required to access this resource.");
            return;
        }

        if (authorizeResult.Forbidden)
        {
            await WriteProblemAsync(context,
                status: HttpStatusCode.Forbidden,
                code: "forbidden",
                title: "Forbidden",
                detail: "You do not have permission to access this resource.");
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }

    private async Task WriteProblemAsync(
        HttpContext context,
        HttpStatusCode status,
        string code,
        string title,
        string detail)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<CustomAuthorizationMiddlewareResultHandler>>();
        logger.LogWarning("{Code} access attempt at {Path}", code, context.Request.Path);

        var problem = new ProblemDetails
        {
            Type = $"{_baseUrl}/{code}",
            Title = title,
            Detail = detail,
            Status = (int)status,
            Instance = context.TraceIdentifier,
            Extensions =
            {
                ["error_code"] = code,
                ["trace_id"] = context.TraceIdentifier
            }
        };

        context.Response.StatusCode = problem.Status.Value;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        await context.Response.WriteAsync(json);
    }
}
