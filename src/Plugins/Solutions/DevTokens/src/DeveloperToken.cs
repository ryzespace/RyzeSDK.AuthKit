using Core.TokenKeyBindings.Services;
using DevTokens.ValueObject;

namespace DevTokens;

/// <summary>
/// Represents a developer issued token with name, scopes, signing key bindings, and lifetime.
/// </summary>
/// <remarks>
/// <para>
/// A developer token identifies a specific developer and contains the scopes
/// and signing key bindings associated with that token.
/// The token lifetime defines when the token was created and, optionally, when it expires.
/// </para>
/// <para>
/// Token modifications return new instances instead of mutating the existing token,
/// allowing token state to be treated immutably.
/// </para>
/// </remarks>
public record DeveloperToken
{
    /// <summary>
    /// Gets the unique identifier of the developer token.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the unique identifier of the developer who owns the token.
    /// </summary>
    public Guid DeveloperId { get; init; }

    /// <summary>
    /// Gets the name assigned to the token.
    /// </summary>
    public TokenName Name { get; init; } = new("default");

    /// <summary>
    /// Gets the scopes granted to the token.
    /// </summary>
    public IReadOnlyList<TokenScope> Scopes { get; init; } = [];

    /// <summary>
    /// Gets the signing key bindings associated with the token.
    /// </summary>
    public IReadOnlyList<TokenKeyBinding> KeyBindings { get; init; } = [];

    /// <summary>
    /// Gets the lifetime information for the token.
    /// </summary>
    public TokenLifetime Lifetime { get; init; } = new(DateTimeOffset.UtcNow);

    /// <summary>
    /// Creates new developer token.
    /// </summary>
    /// <param name="developerId">The unique identifier of the developer who owns the token.</param>
    /// <param name="name">The name assigned to the token.</param>
    /// <param name="scopes">The scopes granted to the token.</param>
    /// <param name="lifetime">
    /// Optional token lifetime. If <c>null</c>, the token does not expire.
    /// </param>
    /// <returns>A newly created <see cref="DeveloperToken"/>.</returns>
    public static DeveloperToken Create(
        Guid developerId,
        TokenName name,
        IEnumerable<TokenScope> scopes,
        TimeSpan? lifetime = null)
    {
        var now = DateTimeOffset.UtcNow;

        var tokenLifetime = new TokenLifetime(
            now,
            lifetime.HasValue ? now.Add(lifetime.Value) : null
        );

        return new DeveloperToken
        {
            DeveloperId = developerId,
            Name = name,
            Scopes = scopes.ToList().AsReadOnly(),
            Lifetime = tokenLifetime
        };
    }

    /// <summary>
    /// Renews the token with a new lifetime starting from the current time.
    /// </summary>
    /// <param name="extension">The duration for which the renewed token remains valid.</param>
    /// <returns>A new <see cref="DeveloperToken"/> with the renewed lifetime.</returns>
    public DeveloperToken Renew(TimeSpan extension)
    {
        var now = DateTimeOffset.UtcNow;
        var newLifetime = new TokenLifetime(now, now.Add(extension));

        return this with { Lifetime = newLifetime };
    }

    /// <summary>
    /// Adds scope to the token.
    /// </summary>
    /// <param name="scope">The scope to add.</param>
    /// <returns>A new <see cref="DeveloperToken"/> containing the additional scope.</returns>
    public DeveloperToken AddScope(TokenScope scope)
    {
        var newScopes = Scopes
            .Append(scope)
            .ToList()
            .AsReadOnly();

        return this with { Scopes = newScopes };
    }

    /// <summary>
    /// Adds signing key binding to the token.
    /// </summary>
    /// <param name="binding">The signing key binding to add.</param>
    /// <returns>A new <see cref="DeveloperToken"/> containing the additional key binding.</returns>
    public DeveloperToken AddKeyBinding(TokenKeyBinding binding)
    {
        var newBindings = KeyBindings
            .Append(binding)
            .ToList()
            .AsReadOnly();

        return this with { KeyBindings = newBindings };
    }

    /// <summary>
    /// Replaces an existing signing key binding with new binding.
    /// </summary>
    /// <param name="signingKeyId">The identifier of the signing key binding to replace.</param>
    /// <param name="newBinding">The replacement signing key binding.</param>
    /// <returns>A new <see cref="DeveloperToken"/> with the matching key binding replaced.</returns>
    public DeveloperToken ReplaceKeyBinding(
        string signingKeyId,
        TokenKeyBinding newBinding)
    {
        var newBindings = KeyBindings
            .Select(k => k.SigningKeyId == signingKeyId ? newBinding : k)
            .ToList()
            .AsReadOnly();

        return this with { KeyBindings = newBindings };
    }
}