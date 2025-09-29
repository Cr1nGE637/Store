namespace Store.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }

    private User(Guid id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
    }

    public static User Create(Guid id, string name,  string email)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(email));
        }
        var user = new User(id, name, email);
        return user;
    }
}