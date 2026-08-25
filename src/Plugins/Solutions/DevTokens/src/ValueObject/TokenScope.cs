using System.Text.Json.Serialization;

namespace DevTokens.ValueObject;

/// <summary>
/// Represents a normalized scope assigned to the developer token.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>The scope name cannot be <c>null</c>, empty, or consist only of whitespace. </item>
/// <item>Leading and trailing whitespace is removed during object creation. </item>
/// <item>The scope name is normalized to lowercase using the invariant culture.</item>
/// <item>Supports implicit conversion from <see cref="string"/> to <see cref="TokenScope"/>.</item>
/// <item>Supports implicit conversion from <see cref="TokenScope"/> to <see cref="string"/>.</item>
/// </list>
/// </remarks>
public readonly record struct TokenScope
{
    /// <summary>
    /// Gets the normalized scope name.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="TokenScope"/> struct.
    /// </summary>
    /// <param name="value">The scope name to validate and normalize.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <c>null</c>, empty,
    /// or consists only of whitespace.
    /// </exception>
    [JsonConstructor]
    public TokenScope(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Scope name cannot be null or empty.",
                nameof(value));

        Value = value.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Implicitly converts <see cref="string"/> to <see cref="TokenScope"/>.
    /// </summary>
    /// <param name="s">The scope name to convert.</param>
    public static implicit operator TokenScope(string s) => new(s);

    /// <summary>
    /// Implicitly converts <see cref="TokenScope"/> to <see cref="string"/>.
    /// </summary>
    /// <param name="s">The scope to convert.</param>
    public static implicit operator string(TokenScope s) => s.Value;

    /// <summary>
    /// Returns the normalized scope name.
    /// </summary>
    /// <returns>The underlying scope value.</returns>
    public override string ToString() => Value;
}