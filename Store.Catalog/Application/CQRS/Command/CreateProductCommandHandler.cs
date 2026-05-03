using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
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

        var product = productResult.Value;
        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in product.DomainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);
        product.ClearDomainEvents();

        return Result.Success(CatalogMappings.ToCreateProductDto(product));
    }
}
