namespace Domain.Entities;

public class DeveloperTokenBlock
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TokenId { get; private set; }
    public string Reason { get; private set; }
    public string? BlockedBy { get; private set; }
    public DateTime BlockedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; private set; }
    
    public DeveloperTokenBlock(Guid tokenId, string reason, string? blockedBy = null, DateTime? expiresAt = null)
    {
        TokenId = tokenId;
        Reason = reason;
        BlockedBy = blockedBy;
        ExpiresAt = expiresAt;
    }

    public bool IsActive() => !ExpiresAt.HasValue || ExpiresAt.Value > DateTime.UtcNow;
}