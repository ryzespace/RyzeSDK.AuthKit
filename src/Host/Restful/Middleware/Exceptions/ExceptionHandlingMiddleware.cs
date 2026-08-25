using System.Net;
using System.Text.Json;
using Core;
using Core.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Host.Restful.Middleware.Exceptions;

/// <summary>
/// Middleware that handles unhandled exceptions and returns standardized
/// RFC 7807 <see cref="ProblemDetails"/> responses.
/// </summary>
/// <remarks>
/// <para>
/// Domain specific exceptions represented by <see cref="DomainException"/>
/// are returned as <c>409 Conflict</c> responses with an error code derived
/// from the exception type name.
/// </para>
/// <para>
/// Unexpected exceptions are returned as <c>500 Internal Server Error</c>
/// responses without exposing internal exception details to the client.
/// </para>
/// <para>
/// Problem type URLs are generated using <see cref="ErrorMetadataOptions.DocsBaseUrl"/>
/// and the corresponding error code.
/// </para>
/// </remarks>
public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IOptions<ErrorMetadataOptions> options)
{
    private readonly string _baseUrl = options.Value.DocsBaseUrl.TrimEnd('/');

    /// <summary>
    /// Invokes the next middleware and handles any unhandled exception
    /// produced during request processing.
    /// </summary>
    /// <param name="context">The current HTTP request context.</param>
    /// <returns>A task representing the asynchronous middleware operation.</returns>
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

    /// <summary>
    /// Maps an exception to standardized <see cref="ProblemDetails"/>
    /// response and writes it to the HTTP response.
    /// </summary>
    /// <param name="context">The current HTTP request context.</param>
    /// <param name="ex">The exception that was raised during request processing.</param>
    /// <returns>A task representing the asynchronous response-writing operation.</returns>
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
        {
            logger.LogWarning(
                "Handled domain exception {ErrorCode}: {Message}",
                code,
                message);
        }
        else
        {
            logger.LogError(
                ex,
                "Unhandled exception: {Message}",
                message);
        }

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Maps an exception to its corresponding HTTP status code, error code,
    /// client-safe message, and domain-error indicator.
    /// </summary>
    /// <param name="ex">The exception to map.</param>
    /// <returns>
    /// A tuple containing the HTTP status, error code, response message,
    /// and a value indicating whether the exception is a domain error.
    /// </returns>
    private static (
        HttpStatusCode Status,
        string Code,
        string Message,
        bool IsDomainError) MapException(Exception ex)
    {
        if (ex is not DomainException domainEx)
        {
            return(
                HttpStatusCode.InternalServerError,
                "internal_error",
                "An unexpected error occurred.",
                false);
        }

        var code = ToSnakeCase(domainEx.GetType().Name.Replace("Exception", ""));
        return(
            HttpStatusCode.Conflict,
            code,
            domainEx.Message,
            true);
    }

    /// <summary>
    /// Converts PascalCase or camelCase string to snake_case.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The converted snake_case string.</returns>
    private static string ToSnakeCase(string input) =>
        string.Concat(input.Select((ch, i) =>
            i > 0 && char.IsUpper(ch)
                ? "_" + char.ToLower(ch)
                : char.ToLower(ch).ToString()));
}
