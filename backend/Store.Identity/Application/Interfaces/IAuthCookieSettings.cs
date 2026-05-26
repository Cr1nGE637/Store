namespace Store.Identity.Application.Interfaces;

public interface IAuthCookieSettings
{
    string CookieName { get; }
    DateTimeOffset ExpiresAtUtc();
}
