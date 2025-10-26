using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Restful.Middleware;

/// <summary>
/// Middleware that catches <see cref="ValidationException"/> thrown by FluentValidation
/// and converts them into a standardized HTTP 400 response with JSON error details.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>Intercepts exceptions of type <see cref="ValidationException"/>.</description></item>
/// <item><description>Returns HTTP 400 Bad Request with JSON payload containing property names and error messages.</description></item>
/// <item><description>Should be registered early in the middleware pipeline to catch validation errors from controllers or other middlewares.</description></item>
/// </list>
/// </remarks>
/// <param name="next">The next middleware delegate in the pipeline.</param>
public class ValidationExceptionMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Invokes the middleware, catching <see cref="ValidationException"/> and returning structured JSON errors.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            var errors = ex.Errors
                .Select(e => new { e.PropertyName, e.ErrorMessage });
            await context.Response.WriteAsJsonAsync(new { message = "Validation failed", errors });
        }
    }
}