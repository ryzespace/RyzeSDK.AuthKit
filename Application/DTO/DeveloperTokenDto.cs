using Domain.Entities;

namespace Application.DTO;

public record DeveloperTokenDto
{
    public Guid Id { get; init; }
    public Guid DeveloperId { get; init; }
    public string Name { get; init; } = null!;
    public IReadOnlyList<string> Scopes { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    
    public bool IsExpired => ExpiresAt.HasValue && DateTimeOffset.UtcNow > ExpiresAt.Value;

    public static DeveloperTokenDto FromDomain(DeveloperToken token) =>
        new()
        {
            Id = token.Id,
            DeveloperId = token.DeveloperId,
            Name = token.Name.ToString(),
            Scopes = token.Scopes.Select(s => s.Value).ToList().AsReadOnly(),
            CreatedAt = token.Lifetime.CreatedAt,
            ExpiresAt = token.Lifetime.ExpiresAt
        };
}