using CSharpFunctionalExtensions;

namespace Store.Carts.Domain.ValueObjects;

public sealed record PaymentMethod
{
    public const int MaxLength = 100;

    private PaymentMethod(string value) => Value = value;

    public string Value { get; }

    public static Result<PaymentMethod> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<PaymentMethod>("Payment method is required");

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            return Result.Failure<PaymentMethod>($"Payment method cannot exceed {MaxLength} characters");

        return Result.Success(new PaymentMethod(trimmed));
    }
}
