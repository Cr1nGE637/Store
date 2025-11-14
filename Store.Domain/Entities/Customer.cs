using CSharpFunctionalExtensions;
using Store.Domain.ValueObjects;

namespace Store.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }

    private Customer(Guid id, string name, Email email)
    {
        Id = id;
        Name = name;
        Email = email;
    }

    public static Result<Customer> Create(string name,  Email email, Guid id = default)
    {
        if (id == Guid.Empty)
        {
            id = Guid.NewGuid();
        }
        
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Customer>("Name cannot be empty.");
        }
        
        var user = new Customer(id, name, email);
        return Result.Success(user);
    }
}