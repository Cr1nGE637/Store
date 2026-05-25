using CSharpFunctionalExtensions;

namespace Store.Carts.Domain.ValueObjects;

public sealed record DeliveryMethod
{
    public const int MaxLength = 100;

    private DeliveryMethod(string value) => Value = value;

    public string Value { get; }

    public static Result<DeliveryMethod> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<DeliveryMethod>("Delivery method is required");

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            return Result.Failure<DeliveryMethod>($"Delivery method cannot exceed {MaxLength} characters");

        return Result.Success(new DeliveryMethod(trimmed));
    }
}
