using Microsoft.AspNetCore.Http;

namespace Store.Catalog.API.Requests;

public sealed class UploadProductImageRequest
{
    public IFormFile File { get; init; } = null!;
    public string? AltText { get; init; }
    public bool IsMain { get; init; } = true;
}
