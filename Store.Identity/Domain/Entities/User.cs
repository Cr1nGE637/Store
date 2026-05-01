using CSharpFunctionalExtensions;
using Identity.Domain.Enums;
using Identity.Domain.ValueObjects;
using Store.SharedKernel;

namespace Identity.Domain.Entities;

public class User : AggregateRoot
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Password { get; private set; }
    public Email Email { get; private set; }
    public UserRole Role { get; private set; }

    private User(Guid id, string name, Email email, string password, UserRole role)
    {
        Id = id;
        Name = name;
        Email = email;
        Password = password;
        Role = role;
    }

    public static Result<User> Create(string name, Email email, string password, UserRole role = UserRole.Customer)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<User>("Name cannot be empty.");

        return Result.Success(new User(Guid.NewGuid(), name, email, password, role));
    }

    internal static User Reconstitute(Guid id, string name, Email email, string password, UserRole role) =>
        new(id, name, email, password, role);
}