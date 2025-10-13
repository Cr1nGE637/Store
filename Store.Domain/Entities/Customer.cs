namespace Store.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }

    private Customer(Guid id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
    }

    public static Customer Create(Guid id, string name,  string email)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(email));
        }
        var user = new Customer(id, name, email);
        return user;
    }
}