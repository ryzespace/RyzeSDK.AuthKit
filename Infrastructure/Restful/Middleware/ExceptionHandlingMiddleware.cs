using System.Net;
using System.Text.Json;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Restful.Middleware;

/// <summary>
///  Middleware for handling exceptions and returning standardized RFC 7807 ProblemDetails responses.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Handles <see cref="DomainException"/> with 409 Conflict and domain-specific codes.</item>
/// <item>Handles unknown exceptions with 500 Internal Server Error.</item>
/// <item>Generates a problem type URI (e.g., https://authdev.ryzespace.com/errors/developer_token_limit_exceeded).</item>
/// </list>
/// </remarks>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IOptions<ErrorMetadataOptions> options)
{
    private readonly string _baseUrl = options.Value.DocsBaseUrl.TrimEnd('/');

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, code, message, isDomainError) = MapException(ex);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;

        var problem = new ProblemDetails
        {
            Type = $"{_baseUrl}/{code}",
            Title = code.Replace('_', ' '),
            Detail = message,
            Status = (int)status,
            Instance = context.TraceIdentifier,
            Extensions =
            {
                ["error_code"] = code,
                ["trace_id"] = context.TraceIdentifier
            }
        };

        if (isDomainError)
            logger.LogWarning("Handled domain exception {ErrorCode}: {Message}", code, message);
        else
            logger.LogError(ex, "Unhandled exception: {Message}", message);

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        await context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode Status, string Code, string Message, bool IsDomainError) MapException(Exception ex)
    {
        if (ex is not DomainException domainEx)
            return (HttpStatusCode.InternalServerError, "internal_error", "An unexpected error occurred.", false);
        
        var code = ToSnakeCase(domainEx.GetType().Name.Replace("Exception", ""));
        return (HttpStatusCode.Conflict, code, domainEx.Message, true);

    }

    private static string ToSnakeCase(string input) =>
        string.Concat(input.Select((ch, i) =>
            i > 0 && char.IsUpper(ch) ? "_" + char.ToLower(ch) : char.ToLower(ch).ToString()));
}

/// <summary>
/// Configuration options for error documentation metadata.
/// </summary>
public record ErrorMetadataOptions
{
    public string DocsBaseUrl { get; init; } = "https://localhost:8080/errors";
}
