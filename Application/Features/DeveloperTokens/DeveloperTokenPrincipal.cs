namespace Application.Features.DeveloperTokens;

/// <summary>
/// Represents the authenticated principal of a developer token.
/// </summary>
/// <remarks>
/// Contains developer identity and the set of scopes/permissions granted by the token.
/// Typically used in authorization and rate-limiting middleware.
/// </remarks>
public class DeveloperTokenPrincipal(Guid developerId, string name, IEnumerable<string> scopes)
{
    public Guid DeveloperId { get; } = developerId;
    public string Name { get; } = name;
    public List<string> Scopes { get; } = scopes.ToList();

    public bool HasScope(string scope) => Scopes.Contains(scope);
}
