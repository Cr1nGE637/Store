using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace Store.Carts.Domain.ValueObjects;

public sealed record PhoneNumber
{
    public const int MaxLength = 50;
    private static readonly Regex BasicPhoneRegex = new(@"^\+?[0-9][0-9\s().-]{2,49}$", RegexOptions.Compiled);

    private PhoneNumber(string value) => Value = value;

    public string Value { get; }

    public static Result<PhoneNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<PhoneNumber>("Phone is required");

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            return Result.Failure<PhoneNumber>($"Phone cannot exceed {MaxLength} characters");
        if (!BasicPhoneRegex.IsMatch(trimmed))
            return Result.Failure<PhoneNumber>("Phone format is invalid");

        return Result.Success(new PhoneNumber(trimmed));
    }
}
