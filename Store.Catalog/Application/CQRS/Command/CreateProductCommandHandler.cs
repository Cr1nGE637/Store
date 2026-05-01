using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Contracts.Events;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Command;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<CreateProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public CreateProductCommandHandler(IProductRepository productRepository, ICatalogUnitOfWork unitOfWork, IPublisher publisher)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<Result<CreateProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existing = await _productRepository.GetByNameAsync(request.ProductName);
        if (existing.IsSuccess)
            return Result.Failure<CreateProductDto>("Product already exists");

        var productResult = Product.Create(request.ProductName, request.ProductDescription, request.ProductPrice, request.CategoryId);
        if (productResult.IsFailure)
            return Result.Failure<CreateProductDto>(productResult.Error);

        await _productRepository.AddAsync(productResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new ProductCreatedEvent(
            productResult.Value.ProductId,
            productResult.Value.ProductName,
            productResult.Value.ProductPrice,
            productResult.Value.CategoryId), cancellationToken);

        return Result.Success(MapToDto(productResult.Value));
    }

    private static CreateProductDto MapToDto(Product p) =>
        new(p.ProductId, p.ProductName, p.ProductDescription, p.ProductPrice, p.CategoryId);
}
