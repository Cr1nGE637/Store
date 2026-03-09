using CSharpFunctionalExtensions;
using Users.Domain.ValueObjects;

namespace Users.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Password { get; private set; }
    public Email Email { get; private set; }

    private User(Guid id, string name, Email email, string password)
    {
        Id = id;
        Name = name;
        Email = email;
        Password = password;
    }

    public static Result<User> Create(string name,  Email email, string password, Guid id = default)
    {
        if (id == Guid.Empty)
        {
            id = Guid.NewGuid();
        }
        
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<User>("Name cannot be empty.");
        }
        
        var user = new User(id, name, email, password);
        return Result.Success(user);
    }
}