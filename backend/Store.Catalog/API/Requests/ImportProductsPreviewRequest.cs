using Microsoft.AspNetCore.Http;

namespace Store.Catalog.API.Requests;

public sealed class ImportProductsPreviewRequest
{
    public IFormFile File { get; init; } = null!;
}
