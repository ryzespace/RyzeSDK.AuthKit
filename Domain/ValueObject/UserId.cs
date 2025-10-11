namespace Domain.ValueObject;

public readonly record struct UserId(Guid Value)
{
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(UserId id) => id.Value;
    public static implicit operator UserId(Guid value) => new(value);
    public static UserId New() => new(Guid.NewGuid());
}