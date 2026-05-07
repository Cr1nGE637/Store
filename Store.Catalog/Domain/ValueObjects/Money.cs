using CSharpFunctionalExtensions;

namespace Store.Catalog.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; }

    private Money(decimal amount) => Amount = amount;

    public static Result<Money> Create(decimal amount)
    {
        if (amount < 0)
            return Result.Failure<Money>("Price cannot be negative");

        return Result.Success(new Money(amount));
    }

    internal static Money Reconstitute(decimal amount)
    {
        var result = Create(amount);
        if (result.IsFailure)
            throw new InvalidOperationException($"Corrupt Money in storage: {result.Error}");
        return result.Value;
    }

    public static implicit operator decimal(Money money) => money.Amount;

    public override string ToString() => Amount.ToString("F2");

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
    }
}