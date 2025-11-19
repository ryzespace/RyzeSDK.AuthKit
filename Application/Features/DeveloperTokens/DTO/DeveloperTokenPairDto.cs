namespace Application.Features.DeveloperTokens.DTO;

/// <summary>
/// Data transfer object representing a generated developer token pair.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><term>ShortKey</term> – public identifier used as API key in requests.</item>
/// <item><term>Jwt</term> – signed JSON Web Token containing developer claims.</item>
/// </list>
/// </remarks>
public record DeveloperTokenPairDto(string ShortKey, string Jwt);