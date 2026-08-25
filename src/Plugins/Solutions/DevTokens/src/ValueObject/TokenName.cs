namespace DevTokens.ValueObject;

/// <summary>
/// Represents the name assigned to a developer token.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>The value cannot be <c>null</c>, empty, or consist only of whitespace.</item>
/// <item>The value is limited to a maximum of 100 characters.</item>
/// <item>Leading and trailing whitespace is removed during object creation.</item>
/// <item>Supports implicit conversion from <see cref="string"/> to <see cref="TokenName"/>.</item>
/// <item>Supports implicit conversion from <see cref="TokenName"/> to <see cref="string"/>.</item>
/// </list>
/// </remarks>
public readonly record struct TokenName(string Value)
{
    /// <summary>
    /// Returns the token name as a string.
    /// </summary>
    /// <returns>The underlying token name value.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts <see cref="TokenName"/> to <see cref="string"/>.
    /// </summary>
    /// <param name="t">The token name to convert.</param>
    public static implicit operator string(TokenName t) => t.Value;

    /// <summary>
    /// Implicitly converts <see cref="string"/> to <see cref="TokenName"/>.
    /// </summary>
    /// <param name="s">The string value to convert.</param>
    public static implicit operator TokenName(string s) => new(s);
}