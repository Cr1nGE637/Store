using CSharpFunctionalExtensions;

namespace Store.Carts.Domain.ValueObjects;

public sealed record DeliveryAddress
{
    public const int MaxLength = 500;

    private DeliveryAddress(string value) => Value = value;

    public string Value { get; }

    public static Result<DeliveryAddress> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<DeliveryAddress>("Delivery address is required");

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            return Result.Failure<DeliveryAddress>($"Delivery address cannot exceed {MaxLength} characters");

        return Result.Success(new DeliveryAddress(trimmed));
    }
}
