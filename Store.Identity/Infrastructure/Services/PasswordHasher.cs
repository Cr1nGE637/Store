using Store.Identity.Application.Interfaces;

namespace Store.Identity.Infrastructure.Services;

public class PasswordHasher :  IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Generate(string password) => 
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hashedPassword) =>
        BCrypt.Net.BCrypt.Verify(password, hashedPassword);
}
