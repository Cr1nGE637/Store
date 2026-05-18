using CSharpFunctionalExtensions;

namespace Store.Catalog.Domain.ValueObjects;

public class Sku : ValueObject
{
    public string Value { get; }

    private Sku(string value) => Value = value;

    public static Result<Sku> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Sku>("SKU is required");

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length > 64)
            return Result.Failure<Sku>("SKU cannot exceed 64 characters");

        return Result.Success(new Sku(normalized));
    }

    internal static Sku Reconstitute(string value)
    {
        var result = Create(value);
        if (result.IsFailure)
            throw new InvalidOperationException($"Corrupt SKU in storage: {result.Error}");

        return result.Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Sku sku) => sku.Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
