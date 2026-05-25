using System.Text;
using Microsoft.Extensions.Options;

namespace Store.Identity.Infrastructure.Configuration;

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    private const int MinimumSecretKeyBytes = 32;

    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            errors.Add("JwtOptions.SecretKey is required.");
        }
        else if (Encoding.UTF8.GetByteCount(options.SecretKey) < MinimumSecretKeyBytes)
        {
            errors.Add($"JwtOptions.SecretKey must be at least {MinimumSecretKeyBytes} bytes.");
        }

        if (string.IsNullOrWhiteSpace(options.Issuer))
            errors.Add("JwtOptions.Issuer is required.");

        if (string.IsNullOrWhiteSpace(options.Audience))
            errors.Add("JwtOptions.Audience is required.");

        if (options.ExpiresHours <= 0)
            errors.Add("JwtOptions.ExpiresHours must be greater than zero.");

        return errors.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(errors);
    }
}
