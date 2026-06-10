namespace Store.Consulting.Infrastructure.Entity;

public sealed class ConsultingCatalogProductEntity
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public ConsultingCatalogCategoryEntity Category { get; set; } = null!;
    public List<ConsultingCatalogProductSpecificationEntity> Specifications { get; set; } = [];
}
