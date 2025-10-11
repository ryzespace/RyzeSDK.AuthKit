namespace Domain.ValueObject;

public readonly record struct SessionId(Guid Value)
{
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(SessionId id) => id.Value;
    public static implicit operator SessionId(Guid value) => new(value);
}