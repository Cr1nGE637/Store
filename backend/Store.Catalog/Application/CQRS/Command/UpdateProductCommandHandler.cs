using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;
using Store.Catalog.Domain.Services;

namespace Store.Catalog.Application.CQRS.Command;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<GetProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly ICatalogDomainEventOutbox _outbox;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ICatalogUnitOfWork unitOfWork,
        ICatalogDomainEventOutbox outbox)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _outbox = outbox;
    }

    public async Task<Result<GetProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var existingResult = await _productRepository.GetByIdAsync(request.ProductId);
        if (existingResult.IsFailure)
            return Result.Failure<GetProductDto>("Product not found");

        var duplicateName = await _productRepository.GetByNameAsync(request.ProductName);
        if (duplicateName.IsSuccess && duplicateName.Value.ProductId != request.ProductId)
            return Result.Failure<GetProductDto>("Product already exists");

        var duplicateSku = await _productRepository.GetBySkuAsync(request.Sku);
        if (duplicateSku.IsSuccess && duplicateSku.Value.ProductId != request.ProductId)
            return Result.Failure<GetProductDto>("Product SKU already exists");

        var categoryResult = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (categoryResult.IsFailure)
            return Result.Failure<GetProductDto>("Category not found");

        var specificationRulesResult = ElectronicsCategorySpecificationRules.ValidateRequiredSpecifications(
            categoryResult.Value.CategoryCode,
            request.Specifications);
        if (specificationRulesResult.IsFailure)
            return Result.Failure<GetProductDto>(specificationRulesResult.Error);

        var product = existingResult.Value;
        var updateResult = product.Update(
            request.Sku,
            request.ProductName,
            request.ProductDescription,
            request.ProductPrice,
            request.Brand,
            request.Model,
            request.WarrantyMonths,
            request.CategoryId,
            request.Specifications);
        if (updateResult.IsFailure)
            return Result.Failure<GetProductDto>(updateResult.Error);
        if (!updateResult.Value)
            return Result.Success(CatalogMappings.ToGetProductDto(product));

        var saveResult = await _productRepository.UpdateAsync(product);
        if (saveResult.IsFailure)
            return Result.Failure<GetProductDto>(saveResult.Error);

        await _outbox.AddAsync(product.DomainEvents, cancellationToken);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (CatalogUniqueConstraintViolationException)
        {
            return Result.Failure<GetProductDto>("Product already exists");
        }

        product.ClearDomainEvents();

        return Result.Success(CatalogMappings.ToGetProductDto(product));
    }
}
