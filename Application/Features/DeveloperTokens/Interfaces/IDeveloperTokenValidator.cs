namespace Application.Features.DeveloperTokens.Interfaces;

/// <summary>
/// Interface for validating developer JWT tokens.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Provides a method to validate a developer token string.</item>
/// <item>Returns a <see cref="DeveloperTokenPrincipal"/> if the token is valid, otherwise null.</item>
/// </list>
/// </remarks>
public interface IDeveloperTokenValidator
{
    /// <summary>
    /// Validates the specified developer token asynchronously.
    /// </summary>
    /// <param name="token">The JWT token string to validate.</param>
    /// <returns>
    /// A task that returns a <see cref="DeveloperTokenPrincipal"/> if the token is valid,
    /// or <c>null</c> if the token is invalid.
    /// </returns>
    Task<DeveloperTokenPrincipal?> ValidateAsync(string token);
}