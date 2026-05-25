namespace Store.Catalog.API.Requests;

public class CreateCategoryRequest
{
    public string CategoryName { get; init; } = string.Empty;
    public string CategoryCode { get; init; } = string.Empty;
}
