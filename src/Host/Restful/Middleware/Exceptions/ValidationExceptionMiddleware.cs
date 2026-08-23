using System.Net;
using System.Text.Json;
using Core.Options;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Host.Restful.Middleware.Exceptions;

/// <summary>
/// Middleware that handles <see cref="ValidationException"/> instances and returns
/// standardized RFC 7807 <see cref="ProblemDetails"/> responses.
/// </summary>
/// <remarks>
/// <para>
/// Validation failures are returned as <c>400 Bad Request</c> responses containing
/// structured validation errors grouped by property name.
/// </para>
/// <para>
/// Uses <see cref="ErrorMetadataOptions.DocsBaseUrl"/> to generate the problem
/// documentation URL.
/// </para>
/// <para>
/// Validation failures are logged together with the request path and validation
/// error details for diagnostics.
/// </para>
/// </remarks>
public sealed class ValidationExceptionMiddleware(
    RequestDelegate next,
    ILogger<ValidationExceptionMiddleware> logger,
    IOptions<ErrorMetadataOptions> options)
{
    private readonly string _baseUrl = options.Value.DocsBaseUrl.TrimEnd('/');

    /// <summary>
    /// Invokes the next middleware and handles any <see cref="ValidationException"/>
    /// raised during request processing.
    /// </summary>
    /// <param name="context">The current HTTP request context.</param>
    /// <returns>A task representing the asynchronous middleware operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationAsync(context, ex);
        }
    }

    /// <summary>
    /// Creates and writes standardized <see cref="ProblemDetails"/> response
    /// containing the validation errors.
    /// </summary>
    /// <param name="context">The current HTTP request context.</param>
    /// <param name="ex">The validation exception containing the validation failures.</param>
    /// <returns>A task representing the asynchronous response-writing operation.</returns>
    private async Task HandleValidationAsync(
        HttpContext context,
        ValidationException ex)
    {
        const string code = "validation_failed";
        const HttpStatusCode status = HttpStatusCode.BadRequest;

        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(e => e.ErrorMessage)
                    .ToArray());

        var problem = new ProblemDetails
        {
            Type = $"{_baseUrl}/{code}",
            Title = "Validation failed",
            Detail = "One or more validation errors occurred.",
            Status = (int)status,
            Instance = context.TraceIdentifier,
            Extensions =
            {
                ["error_code"] = code,
                ["trace_id"] = context.TraceIdentifier,
                ["errors"] = errors
            }
        };

        logger.LogWarning(
            "Validation failed on {Path}: {@Errors}",
            context.Request.Path,
            errors);

        context.Response.StatusCode = problem.Status!.Value;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        await context.Response.WriteAsync(json);
    }
}
