namespace Domain.ValueObject;

public readonly record struct GroupId(Guid Value)
{
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(GroupId id) => id.Value;
    public static implicit operator GroupId(Guid value) => new(value);
}