namespace DevTokens.ValueObject;

/// <summary>
/// Represents the lifetime of developer token, including its creation time
/// and optional expiration time.
/// </summary>
/// <remarks>
/// <para>
/// A token lifetime may be finite or unlimited. When <see cref="ExpiresAt"/>
/// is <c>null</c>, the token does not have an expiration time.
/// </para>
/// <list type="bullet">
/// <item><see cref="CreatedAt"/> specifies when the token was created.</item>
/// <item><see cref="ExpiresAt"/> specifies when the token expires, or <c>null</c> when the token never expires. </item>
/// <item><see cref="Days"/> returns the lifetime duration in whole days when an expiration time is configured. </item>
/// <item><see cref="IsExpired"/> indicates whether the token has passed its expiration time. </item>
/// <item><see cref="Remaining"/> returns the time remaining until expiration, or <c>null</c> for tokens without an expiration time.</item>
/// </list>
/// </remarks>
public sealed record TokenLifetime
{
    /// <summary>
    /// Gets the date and time when the token was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the date and time when the token expires.
    /// A <c>null</c> value indicates that the token never expires.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    /// Initializes new instance of the <see cref="TokenLifetime"/> record.
    /// </summary>
    /// <param name="createdAt">The date and time when the token was created.</param>
    /// <param name="expiresAt">The optional expiration date and time of the token.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="expiresAt"/> is earlier than <paramref name="createdAt"/></exception>
    public TokenLifetime(
        DateTimeOffset createdAt,
        DateTimeOffset? expiresAt = null)
    {
        if (expiresAt.HasValue && expiresAt.Value < createdAt)
        {
            throw new ArgumentException(
                "Expiration date cannot be earlier than creation date.",
                nameof(expiresAt));
        }

        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Gets the configured lifetime in days.
    /// </summary>
    /// <value>
    /// The number of days between creation and expiration, or <c>null</c>
    /// when the token does not expire.
    /// </value>
    public int? Days => ExpiresAt.HasValue
        ? (int?)(ExpiresAt.Value - CreatedAt).TotalDays
        : null;

    /// <summary>
    /// Gets value indicating whether the token has expired.
    /// </summary>
    /// <value>
    /// <c>true</c> when the token has an expiration time that has passed;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool IsExpired =>
        ExpiresAt.HasValue &&
        DateTimeOffset.UtcNow > ExpiresAt.Value;

    /// <summary>
    /// Gets the remaining time until the token expires.
    /// </summary>
    /// <value>The remaining duration until expiration, or <c>null</c> when the token does not expire.</value>
    public TimeSpan? Remaining =>
        ExpiresAt.HasValue
            ? ExpiresAt.Value - DateTimeOffset.UtcNow
            : null;

    /// <summary>
    /// Returns string representation of the token lifetime.
    /// </summary>
    /// <returns>
    /// Formatted range containing the creation and expiration times,
    /// or <c>never</c> when no expiration time is configured.
    /// </returns>
    public override string ToString() =>
        ExpiresAt.HasValue
            ? $"{CreatedAt:u} - {ExpiresAt:u}"
            : $"{CreatedAt:u} - never";
}