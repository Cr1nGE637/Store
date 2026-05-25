using CSharpFunctionalExtensions;

namespace Store.Ordering.Domain.ValueObjects;

public sealed record RecipientName
{
    public const int MaxLength = 200;

    private RecipientName(string value) => Value = value;

    public string Value { get; }

    public static Result<RecipientName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<RecipientName>("Recipient name is required");

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            return Result.Failure<RecipientName>($"Recipient name cannot exceed {MaxLength} characters");

        return Result.Success(new RecipientName(trimmed));
    }
}
