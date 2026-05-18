using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Command;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<CreateProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly ICatalogDomainEventOutbox _outbox;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICatalogUnitOfWork unitOfWork,
        ICatalogDomainEventOutbox outbox)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _outbox = outbox;
    }

    public async Task<Result<CreateProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existing = await _productRepository.GetByNameAsync(request.ProductName);
        if (existing.IsSuccess)
            return Result.Failure<CreateProductDto>("Product already exists");

        var existingSku = await _productRepository.GetBySkuAsync(request.Sku);
        if (existingSku.IsSuccess)
            return Result.Failure<CreateProductDto>("Product SKU already exists");

        var productResult = Product.Create(
            request.Sku,
            request.ProductName,
            request.ProductDescription,
            request.ProductPrice,
            request.Brand,
            request.Model,
            request.WarrantyMonths,
            request.CategoryId,
            request.Specifications);
        if (productResult.IsFailure)
            return Result.Failure<CreateProductDto>(productResult.Error);

        var product = productResult.Value;
        await _productRepository.AddAsync(product);
        await _outbox.AddAsync(product.DomainEvents, cancellationToken);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (CatalogPersistenceErrors.IsUniqueViolation(ex))
        {
            return Result.Failure<CreateProductDto>("Product already exists");
        }

        product.ClearDomainEvents();

        return Result.Success(CatalogMappings.ToCreateProductDto(product));
    }
}
