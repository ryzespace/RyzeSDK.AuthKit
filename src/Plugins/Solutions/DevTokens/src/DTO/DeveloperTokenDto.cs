namespace DevTokens.DTO;

/// <summary>
/// Represents the data transfer object for developer token.
/// </summary>
/// <remarks>
/// <para>
/// Exposes the public token metadata required by API consumers without
/// exposing the underlying domain model.
/// </para>
/// <para>
/// Provides calculated expiration state and a factory method for mapping
/// <see cref="DeveloperToken"/> domain entity to its DTO representation.
/// </para>
/// </remarks>
public record DeveloperTokenDto
{
    /// <summary>
    /// Gets the unique identifier of the developer token.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the unique identifier of the developer that owns the token.
    /// </summary>
    public Guid DeveloperId { get; init; }

    /// <summary>
    /// Gets the display name of the developer token.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Gets the scopes granted to the developer token.
    /// </summary>
    public IReadOnlyList<string> Scopes { get; init; } = [];

    /// <summary>
    /// Gets the date and time when the developer token was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the date and time when the developer token expires,
    /// or <c>null</c> when the token does not expire.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    /// Gets value indicating whether the developer token has expired.
    /// </summary>
    public bool IsExpired =>
        ExpiresAt.HasValue && DateTimeOffset.UtcNow > ExpiresAt.Value;

    /// <summary>
    /// Creates <see cref="DeveloperTokenDto"/> from domain token.
    /// </summary>
    /// <param name="token">The developer token domain entity to map.</param>
    /// <returns>A DTO containing the public representation of the developer token.</returns>
    public static DeveloperTokenDto FromDomain(DeveloperToken token) =>
        new()
        {
            Id = token.Id,
            DeveloperId = token.DeveloperId,
            Name = token.Name.ToString(),
            Scopes = token.Scopes
                .Select(s => s.Value)
                .ToList()
                .AsReadOnly(),
            CreatedAt = token.Lifetime.CreatedAt,
            ExpiresAt = token.Lifetime.ExpiresAt
        };
}