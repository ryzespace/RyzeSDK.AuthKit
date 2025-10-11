namespace Domain.ValueObject;

public readonly record struct RoleId(Guid Value)
{
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(RoleId id) => id.Value;
    public static implicit operator RoleId(Guid value) => new(value);
}