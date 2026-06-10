using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Contracts.Events;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Command;

public sealed class UploadProductImageCommandHandler(
    IProductRepository productRepository,
    IProductImageRepository imageRepository,
    IProductImageStorage imageStorage,
    ICatalogDomainEventOutbox outbox,
    ICatalogUnitOfWork unitOfWork)
    : IRequestHandler<UploadProductImageCommand, Result<ProductImageDto>>
{
    public async Task<Result<ProductImageDto>> Handle(
        UploadProductImageCommand request,
        CancellationToken cancellationToken)
    {
        var productResult = await productRepository.GetByIdAsync(request.ProductId);
        if (productResult.IsFailure)
            return Result.Failure<ProductImageDto>(productResult.Error);

        var storageResult = await imageStorage.SaveAsync(
            request.ProductId,
            request.Content,
            request.OriginalFileName,
            request.ContentType,
            request.SizeBytes,
            cancellationToken);
        if (storageResult.IsFailure)
            return Result.Failure<ProductImageDto>(storageResult.Error);

        var displayOrder = await imageRepository.GetNextDisplayOrderAsync(request.ProductId, cancellationToken);
        var imageResult = ProductImage.Create(
            request.ProductId,
            storageResult.Value.Url,
            storageResult.Value.StoragePath,
            storageResult.Value.OriginalFileName,
            storageResult.Value.ContentType,
            storageResult.Value.SizeBytes,
            request.AltText,
            request.IsMain,
            displayOrder);
        if (imageResult.IsFailure)
        {
            await imageStorage.DeleteAsync(storageResult.Value.StoragePath, cancellationToken);
            return Result.Failure<ProductImageDto>(imageResult.Error);
        }

        if (request.IsMain)
            await imageRepository.ClearMainImageAsync(request.ProductId, cancellationToken);

        try
        {
            await imageRepository.AddAsync(imageResult.Value, cancellationToken);
            if (imageResult.Value.IsMain)
            {
                await outbox.AddAsync(
                    [
                        new ProductMainImageChangedEvent(
                            request.ProductId,
                            imageResult.Value.Url,
                            imageResult.Value.AltText)
                    ],
                    cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await imageStorage.DeleteAsync(storageResult.Value.StoragePath, cancellationToken);
            throw;
        }

        return await imageRepository.GetByIdAsync(imageResult.Value.ProductImageId, cancellationToken);
    }
}
