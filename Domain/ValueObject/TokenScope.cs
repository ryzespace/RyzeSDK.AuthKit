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
public sealed record TokenScope
{
    public string Name { get; }

    private TokenScope(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Scope name cannot be null or empty.", nameof(name));

        Name = name.Trim().ToLowerInvariant();
    }
    
    public static implicit operator TokenScope(string value) => new(value);
    public override string ToString() => Name;
}