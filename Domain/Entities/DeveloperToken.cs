namespace Domain.Entities;

public class DeveloperToken
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid DeveloperId { get; init; }
    public string Name { get; init; } = null!;
    public List<string> Scopes { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; init; }

    public bool IsExpired => ExpiresAt.HasValue && DateTimeOffset.UtcNow > ExpiresAt.Value;
}