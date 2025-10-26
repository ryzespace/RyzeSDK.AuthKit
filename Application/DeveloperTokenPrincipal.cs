namespace Application;

public class DeveloperTokenPrincipal(Guid developerId, string name, IEnumerable<string> scopes)
{
    public Guid DeveloperId { get; } = developerId;
    public string Name { get; } = name;
    public List<string> Scopes { get; } = scopes.ToList();

    public bool HasScope(string scope) => Scopes.Contains(scope);
}
