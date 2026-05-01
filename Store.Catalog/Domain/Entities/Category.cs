using CSharpFunctionalExtensions;
using Store.SharedKernel;

namespace Store.Catalog.Domain.Entities;

public class Category : AggregateRoot
{
    public Guid CategoryId { get; private set; }
    public string CategoryName { get; private set; }

    private Category(Guid categoryId, string categoryName)
    {
        CategoryId = categoryId;
        CategoryName = categoryName;
    }

    public static Result<Category> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Category>("Category name is required");

        return Result.Success(new Category(Guid.NewGuid(), name));
    }

    internal static Category Reconstitute(Guid id, string name) => new(id, name);

    public Result Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Category name is required");

        CategoryName = name;
        return Result.Success();
    }
}
