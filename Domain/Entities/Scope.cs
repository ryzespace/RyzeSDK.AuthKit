namespace Domain.Entities;

public class Scope
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public bool IsDefault { get; private set; }

    private Scope() { }

    public static Scope Define(string name, string description, bool isDefault = false)
        => new() { Name = name, Description = description, IsDefault = isDefault };
}