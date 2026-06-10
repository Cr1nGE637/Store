using CSharpFunctionalExtensions;

namespace Store.Catalog.Application.Interfaces;

public interface IProductImageStorage
{
    Task<Result<ProductImageFileResult>> SaveAsync(
        Guid productId,
        Stream content,
        string originalFileName,
        string contentType,
        long sizeBytes,
        CancellationToken cancellationToken);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken);
}

public sealed record ProductImageFileResult(
    string Url,
    string StoragePath,
    string OriginalFileName,
    string ContentType,
    long SizeBytes);
