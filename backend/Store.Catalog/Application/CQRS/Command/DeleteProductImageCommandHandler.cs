using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Contracts.Events;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Command;

public sealed class DeleteProductImageCommandHandler(
    IProductRepository productRepository,
    IProductImageRepository imageRepository,
    IProductImageStorage imageStorage,
    ICatalogDomainEventOutbox outbox,
    ICatalogUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProductImageCommand, Result>
{
    public async Task<Result> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var productResult = await productRepository.GetByIdAsync(request.ProductId);
        if (productResult.IsFailure)
            return Result.Failure(productResult.Error);

        var deletedResult = await imageRepository.DeleteAsync(
            request.ProductId,
            request.ProductImageId,
            cancellationToken);
        if (deletedResult.IsFailure)
            return Result.Failure(deletedResult.Error);

        ProductMainImageChangedEvent? mainImageChanged = null;
        if (deletedResult.Value.IsMain)
        {
            var nextMainResult = await imageRepository.SetFirstAvailableAsMainAsync(
                request.ProductId,
                cancellationToken);
            if (nextMainResult.IsFailure)
                return Result.Failure(nextMainResult.Error);

            mainImageChanged = nextMainResult.Value is null
                ? new ProductMainImageChangedEvent(request.ProductId, string.Empty, string.Empty)
                : new ProductMainImageChangedEvent(
                    request.ProductId,
                    nextMainResult.Value.Url,
                    nextMainResult.Value.AltText);
        }

        if (mainImageChanged is not null)
            await outbox.AddAsync([mainImageChanged], cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await imageStorage.DeleteAsync(deletedResult.Value.StoragePath, cancellationToken);

        return Result.Success();
    }
}
