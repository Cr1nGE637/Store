using Microsoft.AspNetCore.Http;

namespace Store.App.API.Requests;

public sealed class ImportProductsRequest
{
    public IFormFile File { get; init; } = null!;
}
