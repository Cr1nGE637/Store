using CSharpFunctionalExtensions;
using Store.Catalog.Domain.Enums;
using Store.SharedKernel;

namespace Store.Catalog.Domain.Entities;

public class Category : AggregateRoot
{
    public Guid CategoryId { get; private set; }
    public string CategoryName { get; private set; }
    public ElectronicsCategoryCode CategoryCode { get; private set; }

    private Category(Guid categoryId, string categoryName, ElectronicsCategoryCode categoryCode)
    {
        CategoryId = categoryId;
        CategoryName = categoryName;
        CategoryCode = categoryCode;
    }

    public static Result<Category> Create(string name, string code)
    {
        var detailsResult = CreateDetails(name, code);
        if (detailsResult.IsFailure)
            return Result.Failure<Category>(detailsResult.Error);

        var details = detailsResult.Value;
        return Result.Success(new Category(Guid.NewGuid(), details.Name, details.Code));
    }

    internal static Category Reconstitute(Guid id, string name, string code)
    {
        var detailsResult = CreateDetails(name, code);
        if (detailsResult.IsFailure)
            throw new InvalidOperationException($"Corrupt category in storage: {detailsResult.Error}");

        var details = detailsResult.Value;
        return new(id, details.Name, details.Code);
    }

    public Result Update(string name, string code)
    {
        var detailsResult = CreateDetails(name, code);
        if (detailsResult.IsFailure)
            return Result.Failure(detailsResult.Error);

        var details = detailsResult.Value;
        CategoryName = details.Name;
        CategoryCode = details.Code;
        return Result.Success();
    }

    private static Result<CategoryDetails> CreateDetails(string name, string code)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<CategoryDetails>("Category name is required");

        var normalizedName = name.Trim();
        if (normalizedName.Length > 100)
            return Result.Failure<CategoryDetails>("Category name cannot exceed 100 characters");

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<CategoryDetails>("Category code is required");

        if (!Enum.TryParse<ElectronicsCategoryCode>(code.Trim(), ignoreCase: true, out var categoryCode)
            || !Enum.IsDefined(typeof(ElectronicsCategoryCode), categoryCode))
            return Result.Failure<CategoryDetails>("Invalid electronics category code");

        return Result.Success(new CategoryDetails(normalizedName, categoryCode));
    }

    private sealed record CategoryDetails(string Name, ElectronicsCategoryCode Code);
}
