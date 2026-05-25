namespace Store.Catalog.Infrastructure.Entity;

public class CategoryEntity
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
}
