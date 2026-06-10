namespace Store.Consulting.Infrastructure.Entity;

public sealed class ConsultingCatalogProductSpecificationEntity
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public ConsultingCatalogProductEntity Product { get; set; } = null!;
}
