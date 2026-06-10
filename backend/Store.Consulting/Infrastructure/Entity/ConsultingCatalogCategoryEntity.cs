namespace Store.Consulting.Infrastructure.Entity;

public sealed class ConsultingCatalogCategoryEntity
{
    public Guid CategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
}
