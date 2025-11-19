using Application.Features.DeveloperTokens.DTO;
using Domain.Features.DeveloperTokens;

namespace Application.Features.DeveloperTokens.Interfaces;

/// <summary>
/// Service responsible for generating JWT tokens for developer tokens.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Provides token generation based on <see cref="DeveloperToken"/> entities.</item>
/// <item>Encapsulates JWT creation and signing logic.</item>
/// </list>
/// </remarks>
public interface IDeveloperTokenService
{
    /// <summary>
    /// Generates a signed JWT string for the specified <see cref="DeveloperToken"/>.
    /// </summary>
    /// <param name="token">The developer token entity for which to generate a JWT.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the signed JWT string.</returns>
    Task<DeveloperTokenPairDto> GenerateToken(DeveloperToken token);
}