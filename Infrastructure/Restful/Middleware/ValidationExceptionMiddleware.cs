using System.Net;
using System.Text.Json;
using Application.Options;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Restful.Middleware;

/// <summary>
/// Middleware that handles <see cref="ValidationException"/> and returns standardized RFC 7807 ProblemDetails responses.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Returns <b>400 Bad Request</b> with validation errors in structured JSON format.</item>
/// <item>Uses <see cref="ErrorMetadataOptions"/> for consistent error documentation links.</item>
/// <item>Ensures consistent error response structure across the entire REST layer.</item>
/// </list>
/// </remarks>
public sealed class ValidationExceptionMiddleware(
    RequestDelegate next,
    ILogger<ValidationExceptionMiddleware> logger,
    IOptions<ErrorMetadataOptions> options)
{
    private readonly string _baseUrl = options.Value.DocsBaseUrl.TrimEnd('/');

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

    private async Task HandleValidationAsync(HttpContext context, ValidationException ex)
    {
        var code = "validation_failed";
        var status = HttpStatusCode.BadRequest;

        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

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

        logger.LogWarning("Validation failed on {Path}: {@Errors}", context.Request.Path, errors);

        context.Response.StatusCode = problem.Status.Value;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        await context.Response.WriteAsync(json);
    }
}
