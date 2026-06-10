using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Contracts.Events;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Command;

public sealed class SetMainProductImageCommandHandler(
    IProductRepository productRepository,
    IProductImageRepository imageRepository,
    ICatalogDomainEventOutbox outbox,
    ICatalogUnitOfWork unitOfWork)
    : IRequestHandler<SetMainProductImageCommand, Result<ProductImageDto>>
{
    public async Task<Result<ProductImageDto>> Handle(
        SetMainProductImageCommand request,
        CancellationToken cancellationToken)
    {
        var productResult = await productRepository.GetByIdAsync(request.ProductId);
        if (productResult.IsFailure)
            return Result.Failure<ProductImageDto>(productResult.Error);

        var imageResult = await imageRepository.SetMainAsync(
            request.ProductId,
            request.ProductImageId,
            cancellationToken);
        if (imageResult.IsFailure)
            return Result.Failure<ProductImageDto>(imageResult.Error);

        await outbox.AddAsync(
            [
                new ProductMainImageChangedEvent(
                    request.ProductId,
                    imageResult.Value.Url,
                    imageResult.Value.AltText)
            ],
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return imageResult;
    }
}
