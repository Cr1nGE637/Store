using Microsoft.Extensions.Options;
using Store.Identity.Application;
using Store.Identity.Application.Interfaces;
using Store.Identity.Infrastructure.Configuration;

namespace Store.Identity.Infrastructure.Services;

public class AuthCookieSettings(IOptions<JwtOptions> jwtOptions) : IAuthCookieSettings
{
    public string CookieName => AuthCookieDefaults.Name;

    public DateTimeOffset ExpiresAtUtc() =>
        DateTimeOffset.UtcNow.AddHours(jwtOptions.Value.ExpiresHours);
}
