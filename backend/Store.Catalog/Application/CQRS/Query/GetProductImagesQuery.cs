using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Query;

public sealed class GetProductImagesQuery : IRequest<Result<IReadOnlyCollection<ProductImageDto>>>
{
    public Guid ProductId { get; init; }
}
