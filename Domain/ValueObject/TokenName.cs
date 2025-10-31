using System.Text.Json.Serialization;

namespace Domain.ValueObject;

/// <summary>
/// Represents the name of a token.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>The value cannot be null, empty, or consist only of whitespace.</item>
/// <item>The length of the value is limited to 100 characters.</item>
/// <item>The value is automatically trimmed during object creation.</item>
/// <item>Supports implicit conversion from <see cref="string"/> to <see cref="TokenName"/>.</item>
/// </list>
/// </remarks>
public readonly record struct TokenName(string Value)
{
    public override string ToString() => Value;
    public static implicit operator string(TokenName t) => t.Value;
    public static implicit operator TokenName(string s) => new(s);
}
