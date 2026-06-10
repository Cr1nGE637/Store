using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Query;

public sealed class GetProductImagesQueryHandler(
    IProductRepository productRepository,
    IProductImageRepository imageRepository)
    : IRequestHandler<GetProductImagesQuery, Result<IReadOnlyCollection<ProductImageDto>>>
{
    public async Task<Result<IReadOnlyCollection<ProductImageDto>>> Handle(
        GetProductImagesQuery request,
        CancellationToken cancellationToken)
    {
        var productResult = await productRepository.GetByIdAsync(request.ProductId);
        if (productResult.IsFailure)
            return Result.Failure<IReadOnlyCollection<ProductImageDto>>(productResult.Error);

        return Result.Success(await imageRepository.GetByProductIdAsync(request.ProductId, cancellationToken));
    }
}
