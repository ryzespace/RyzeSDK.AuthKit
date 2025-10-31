using Domain.ValueObject;

namespace Domain.Entities;

/// <summary>
/// Represents a developer-issued token with a name, scopes, and lifetime.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Each token has a unique <see cref="Id"/>.</item>
/// <item>Associated with a specific developer via <see cref="DeveloperId"/>.</item>
/// <item>Contains a <see cref="Name"/> and a list of <see cref="Scopes"/>.</item>
/// <item>Has a <see cref="Lifetime"/> defining creation and optional expiration.</item>
/// <item>Supports creation, renewal, and adding scopes immutably.</item>
/// </list>
/// </remarks>
public record DeveloperToken
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid DeveloperId { get; init; }

    public TokenName Name { get; init; } = new("default");
    public IReadOnlyList<TokenScope> Scopes { get; init; } = [];
    public TokenLifetime Lifetime { get; init; } = new(DateTimeOffset.UtcNow);
    
    public static DeveloperToken Create(
        Guid developerId,
        TokenName name,
        IEnumerable<TokenScope> scopes,
        TimeSpan? lifetime = null)
    {
        
        var tokenLifetime = new TokenLifetime(
            DateTimeOffset.UtcNow,
            lifetime.HasValue ? DateTimeOffset.UtcNow.Add(lifetime.Value) : null
        );

        return new DeveloperToken
        {
            DeveloperId = developerId,
            Name = name,
            Scopes = scopes.ToList().AsReadOnly(),
            Lifetime = tokenLifetime
        };
    }
    
    public DeveloperToken Renew(TimeSpan extension)
    {
        var newLifetime = new TokenLifetime(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.Add(extension));
        return this with { Lifetime = newLifetime };
    }
    
    public DeveloperToken AddScope(TokenScope scope)
    {
        var newScopes = Scopes.Append(scope).ToList().AsReadOnly();
        return this with { Scopes = newScopes };
    }
}