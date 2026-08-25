using System.Net;
using System.Text.Json;
using Core.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Host.Restful.Middleware.Exceptions;

/// <summary>
/// Handles authorization results and returns standardized RFC 7807
/// <see cref="ProblemDetails"/> responses for unauthorized and forbidden requests.
/// </summary>
/// <remarks>
/// <para>
/// Converts authorization challenges and forbidden results into consistent
/// JSON <see cref="ProblemDetails"/> responses.
/// </para>
/// <para>
/// Uses <see cref="ErrorMetadataOptions"/> to generate documentation URLs
/// for authorization errors.
/// </para>
/// <para>Logs security related authorization failures for auditing and diagnostics.</para>
/// </remarks>
public sealed class CustomAuthorizationMiddlewareResultHandler(
    IOptions<ErrorMetadataOptions> options) : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
    private readonly string _baseUrl = options.Value.DocsBaseUrl.TrimEnd('/');

    /// <summary>
    /// Handles the result of an authorization policy evaluation.
    /// </summary>
    /// <param name="next">The next middleware in the request pipeline.</param>
    /// <param name="context">The current HTTP request context.</param>
    /// <param name="policy">The authorization policy that was evaluated.</param>
    /// <param name="authorizeResult">The result of the authorization policy evaluation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            await WriteProblemAsync(
                context,
                status: HttpStatusCode.Unauthorized,
                code: "unauthorized",
                title: "Unauthorized",
                detail: "Authentication required to access this resource.");

            return;
        }

        if (authorizeResult.Forbidden)
        {
            await WriteProblemAsync(
                context,
                status: HttpStatusCode.Forbidden,
                code: "forbidden",
                title: "Forbidden",
                detail: "You do not have permission to access this resource.");

            return;
        }

        await _defaultHandler.HandleAsync(
            next,
            context,
            policy,
            authorizeResult);
    }

    /// <summary>
    /// Writes standardized RFC 7807 <see cref="ProblemDetails"/> response
    /// for an authorization failure.
    /// </summary>
    /// <param name="context">The current HTTP request context.</param>
    /// <param name="status">The HTTP status code returned to the client.</param>
    /// <param name="code">The application-specific error code.</param>
    /// <param name="title">The human-readable error title.</param>
    /// <param name="detail">The human-readable error description.</param>
    /// <returns>A task representing the asynchronous response-writing operation.</returns>
    private async Task WriteProblemAsync(
        HttpContext context,
        HttpStatusCode status,
        string code,
        string title,
        string detail)
    {
        var logger = context.RequestServices
            .GetRequiredService<
                ILogger<CustomAuthorizationMiddlewareResultHandler>>();

        logger.LogWarning("{Code} access attempt at {Path}",
            code,
            context.Request.Path);

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

        context.Response.StatusCode = problem.Status!.Value;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        await context.Response.WriteAsync(json);
    }
}
