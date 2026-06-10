using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Command;

public sealed class UploadProductImageCommand : IRequest<Result<ProductImageDto>>
{
    public Guid ProductId { get; init; }
    public Stream Content { get; init; } = Stream.Null;
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    public string? AltText { get; init; }
    public bool IsMain { get; init; } = true;
}
