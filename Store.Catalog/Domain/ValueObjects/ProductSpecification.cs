using CSharpFunctionalExtensions;

namespace Store.Catalog.Domain.ValueObjects;

public class ProductSpecification : ValueObject
{
    public string Name { get; }
    public string Value { get; }

    private ProductSpecification(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public static Result<ProductSpecification> Create(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ProductSpecification>("Specification name is required");

        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<ProductSpecification>("Specification value is required");

        var normalizedName = name.Trim();
        var normalizedValue = value.Trim();

        if (normalizedName.Length > 100)
            return Result.Failure<ProductSpecification>("Specification name cannot exceed 100 characters");

        if (normalizedValue.Length > 250)
            return Result.Failure<ProductSpecification>("Specification value cannot exceed 250 characters");

        return Result.Success(new ProductSpecification(normalizedName, normalizedValue));
    }

    internal static ProductSpecification Reconstitute(string name, string value)
    {
        var result = Create(name, value);
        if (result.IsFailure)
            throw new InvalidOperationException($"Corrupt product specification in storage: {result.Error}");

        return result.Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name.ToUpperInvariant();
        yield return Value;
    }
}
