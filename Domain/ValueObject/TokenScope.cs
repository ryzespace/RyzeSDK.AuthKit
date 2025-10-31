using System.Text.Json.Serialization;

namespace Domain.ValueObject;

/// <summary>
/// Represents a scope assigned to a token.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>The scope name cannot be null, empty, or whitespace.</item>
/// <item>The name is automatically trimmed and converted to lowercase invariant.</item>
/// <item>Supports implicit conversion from <see cref="string"/> to <see cref="TokenScope"/>.</item>
/// </list>
/// </remarks>
public readonly record struct TokenScope
{
    public string Value { get; }

    [JsonConstructor]
    public TokenScope(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Scope name cannot be null or empty.", nameof(value));

        Value = value.Trim().ToLowerInvariant();
    }

    public static implicit operator TokenScope(string s) => new(s);
    public static implicit operator string(TokenScope s) => s.Value;

    public override string ToString() => Value;
}