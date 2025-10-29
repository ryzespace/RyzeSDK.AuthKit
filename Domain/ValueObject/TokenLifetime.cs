namespace Domain.ValueObject;

/// <summary>
/// Represents the lifetime of a token, including its creation and optional expiration dates.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><see cref="CreatedAt"/> indicates when the token was created.</item>
/// <item><see cref="ExpiresAt"/> indicates when the token will expire. Null means the token never expires.</item>
/// <item><see cref="IsExpired"/> checks if the token is expired based on the current UTC time.</item>
/// <item><see cref="Remaining"/> returns the remaining time until expiration, or null if the token never expires.</item>
/// </list>
/// </remarks>
public sealed record TokenLifetime
{
    private DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }

    public TokenLifetime(DateTimeOffset createdAt, DateTimeOffset? expiresAt = null)
    {
        if (expiresAt.HasValue && expiresAt.Value < createdAt)
            throw new ArgumentException("Expiration date cannot be earlier than creation date.", nameof(expiresAt));

        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public bool IsExpired => ExpiresAt.HasValue && DateTimeOffset.UtcNow > ExpiresAt.Value;

    public TimeSpan? Remaining => ExpiresAt.HasValue
        ? ExpiresAt.Value - DateTimeOffset.UtcNow
        : null;

    public override string ToString() => ExpiresAt.HasValue 
        ? $"{CreatedAt:u} - {ExpiresAt:u}" 
        : $"{CreatedAt:u} - never";
}