using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Events;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Command;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<GetProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public UpdateProductCommandHandler(IProductRepository productRepository, ICatalogUnitOfWork unitOfWork, IPublisher publisher)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<Result<GetProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var existingResult = await _productRepository.GetByIdAsync(request.ProductId);
        if (existingResult.IsFailure)
            return Result.Failure<GetProductDto>("Product not found");

        var product = existingResult.Value;
        decimal oldPrice = product.ProductPrice;

        var updateResult = product.Update(request.ProductName, request.ProductDescription, request.ProductPrice, request.CategoryId);
        if (updateResult.IsFailure)
            return Result.Failure<GetProductDto>(updateResult.Error);

        var savedResult = await _productRepository.UpdateAsync(product);
        if (savedResult.IsFailure)
            return Result.Failure<GetProductDto>(savedResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (oldPrice != request.ProductPrice)
            await _publisher.Publish(new ProductPriceChangedEvent(request.ProductId, oldPrice, request.ProductPrice), cancellationToken);

        return Result.Success(MapToDto(savedResult.Value));
    }

    private static GetProductDto MapToDto(Product p) =>
        new(p.ProductId, p.ProductName, p.ProductDescription, p.ProductPrice, p.CategoryId);
}
