namespace Application;

public class DeveloperTokenPrincipal
{
    public Guid DeveloperId { get; }
    public string Name { get; }
    public List<string> Scopes { get; }

    public DeveloperTokenPrincipal(Guid developerId, string name, IEnumerable<string> scopes)
     {
        DeveloperId = developerId;
        Name = name;
        Scopes = scopes.ToList();
    }

    public bool HasScope(string scope) => Scopes.Contains(scope);
}
