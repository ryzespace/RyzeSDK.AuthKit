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
public sealed record TokenName
{
    private string Value { get; }

    public TokenName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Token name cannot be null or empty.", nameof(value));
        if (value.Length > 100)
            throw new ArgumentException("Token name cannot exceed 100 characters.", nameof(value));

        Value = value.Trim();
    }

    public static implicit operator TokenName(string value) => new(value);
    public override string ToString() => Value;
}