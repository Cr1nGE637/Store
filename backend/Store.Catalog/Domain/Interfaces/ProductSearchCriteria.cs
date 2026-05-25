namespace Store.Catalog.Domain.Interfaces;

public record ProductSearchCriteria(
    string? Search,
    Guid? CategoryId,
    string? Brand,
    decimal? MinPrice,
    decimal? MaxPrice,
    IReadOnlyDictionary<string, string> SpecificationFilters,
    bool InStockOnly,
    int Skip,
    int Take);
