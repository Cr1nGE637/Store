namespace Store.Catalog.API.Requests;

public class CreateProductRequest
{
    public string Sku { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string ProductDescription { get; init; } = string.Empty;
    public decimal ProductPrice { get; init; }
    public string Brand { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public int WarrantyMonths { get; init; }
    public Guid CategoryId { get; init; }
    public Dictionary<string, string> Specifications { get; init; } = [];
}
