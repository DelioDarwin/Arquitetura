namespace Arquitetura.SharedKernel.Primitives;

/// <summary>
/// Classe base para Value Objects. Dois VOs com os mesmos atributos são iguais.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetAtomicValues();

    public bool Equals(ValueObject? other) =>
        other is not null && GetAtomicValues().SequenceEqual(other.GetAtomicValues());

    public override bool Equals(object? obj) =>
        obj is ValueObject other && Equals(other);

    public override int GetHashCode() =>
        GetAtomicValues()
            .Aggregate(0, (hash, value) => HashCode.Combine(hash, value?.GetHashCode() ?? 0));

    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(ValueObject? left, ValueObject? right) =>
        !(left == right);
}
