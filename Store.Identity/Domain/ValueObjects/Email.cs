using CSharpFunctionalExtensions;

namespace Identity.Domain.ValueObjects;

public class Email : ValueObject
{
    public const int MaxLength = 254;
    public string Value { get; }
    private Email(string value) => Value = value;
    
    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Email>("Email can not be empty.");

        email = email.Trim().ToLowerInvariant();

        if (email.Length > MaxLength)
            return Result.Failure<Email>($"Email can not be longer than {MaxLength} characters.");

        if (!IsValidEmail(email))
            return Result.Failure<Email>("Invalid email.");

        return Result.Success(new Email(email));
    }
    
    private static bool IsValidEmail(string email)
    {
        if (email.Length > MaxLength) return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    public static implicit operator string(Email email) => email?.Value ?? string.Empty;
    
    public override string ToString() => Value;
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}