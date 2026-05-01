using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Contracts.Events;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Command;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<GetProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public DeleteProductCommandHandler(IProductRepository productRepository, ICatalogUnitOfWork unitOfWork, IPublisher publisher)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<Result<GetProductDto>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var productResult = await _productRepository.GetByIdAsync(request.ProductId);
        if (productResult.IsFailure)
            return Result.Failure<GetProductDto>("Product not found");

        var deleteResult = await _productRepository.DeleteAsync(productResult.Value.ProductId);
        if (deleteResult.IsFailure)
            return Result.Failure<GetProductDto>(deleteResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new ProductDeletedEvent(
            productResult.Value.ProductId,
            productResult.Value.ProductName), cancellationToken);

        return Result.Success(MapToDto(productResult.Value));
    }

    private static GetProductDto MapToDto(Product p) =>
        new(p.ProductId, p.ProductName, p.ProductDescription, p.ProductPrice, p.CategoryId);
}
