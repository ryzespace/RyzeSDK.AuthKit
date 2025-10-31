using Domain.Entities;

namespace Application.DTO;

/// <summary>
/// Represents the result of a developer token creation.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Contains the signed JWT string for the created token.</item>
/// <item>Includes the <see cref="DeveloperToken"/> entity with all details.</item>
/// </list>
/// </remarks>
public class DeveloperTokenCreated
{
    public string Jwt { get; set; } = null!;
    public DeveloperToken Token { get; set; } = null!;
}