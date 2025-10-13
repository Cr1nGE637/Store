using CSharpFunctionalExtensions;

namespace Store.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public string Description { get; private set; }

    private Product(Guid id, string name, string description, decimal price)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
    }

    public static Result<Product> Create(Guid id, string name, string description, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Product>("Product name is required");
        }
        var product = new Product(id, name, description, price);
        return Result.Success(product);
    }
}