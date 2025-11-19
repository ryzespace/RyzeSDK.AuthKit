using Domain.Features.DeveloperTokens;

namespace Application.Features.DeveloperTokens.DTO;

/// <summary>
/// Represents the result of a developer token creation.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Contains the signed JWT string and short API key for the created token.</item>
/// <item>Includes the <see cref="DeveloperToken"/> entity with all details.</item>
/// </list>
/// </remarks>
public class DeveloperTokenCreated
{
    /// <summary>
    /// Short API key used as a public identifier (e.g. "rk_live_xxx").
    /// </summary>
    public string ShortKey { get; set; } = null!;

    /// <summary>
    /// Signed JWT containing developer claims and expiration info.
    /// </summary>
    public string Jwt { get; set; } = null!;
    
    public DeveloperToken Token { get; set; } = null!;
}