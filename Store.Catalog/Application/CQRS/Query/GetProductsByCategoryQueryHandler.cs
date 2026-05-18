using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Query;

public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, Result<List<GetProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductAvailabilityRepository _availabilityRepository;

    public GetProductsByCategoryQueryHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IProductAvailabilityRepository availabilityRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _availabilityRepository = availabilityRepository;
    }

    public async Task<Result<List<GetProductDto>>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var pagination = Pagination.Normalize(request.Page, request.PageSize);
        if (pagination.IsFailure)
            return Result.Failure<List<GetProductDto>>(pagination.Error);

        var categoryResult = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (categoryResult.IsFailure)
            return Result.Failure<List<GetProductDto>>("Category not found");

        var productsResult = await _productRepository.GetByCategoryIdAsync(
            request.CategoryId,
            pagination.Value.Skip,
            pagination.Value.Take);
        if (productsResult.IsFailure)
            return Result.Failure<List<GetProductDto>>(productsResult.Error);

        var availability = await _availabilityRepository.GetAvailableQuantitiesAsync(
            productsResult.Value.Select(product => product.ProductId).ToArray(),
            cancellationToken);

        return Result.Success(productsResult.Value
            .Select(product => CatalogMappings.ToGetProductDto(
                product,
                availability.GetValueOrDefault(product.ProductId)))
            .ToList());
    }
}
