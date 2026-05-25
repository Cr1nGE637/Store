namespace Store.Catalog.Application.Interfaces;

public sealed class CatalogUniqueConstraintViolationException(Exception innerException)
    : Exception("Catalog unique constraint violation", innerException);
