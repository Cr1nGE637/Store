using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Query;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<List<GetProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductAvailabilityRepository _availabilityRepository;

    public GetProductsQueryHandler(
        IProductRepository productRepository,
        IProductAvailabilityRepository availabilityRepository)
    {
        _productRepository = productRepository;
        _availabilityRepository = availabilityRepository;
    }

    public async Task<Result<List<GetProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var pagination = Pagination.Normalize(request.Page, request.PageSize);
        if (pagination.IsFailure)
            return Result.Failure<List<GetProductDto>>(pagination.Error);

        if (request.MinPrice.HasValue && request.MaxPrice.HasValue && request.MinPrice > request.MaxPrice)
            return Result.Failure<List<GetProductDto>>("Minimum price cannot be greater than maximum price");

        var criteria = new ProductSearchCriteria(
            request.Search,
            request.CategoryId,
            request.Brand,
            request.MinPrice,
            request.MaxPrice,
            request.SpecificationFilters,
            request.InStockOnly,
            pagination.Value.Skip,
            pagination.Value.Take);

        var result = await _productRepository.SearchAsync(criteria);
        if (result.IsFailure)
            return Result.Failure<List<GetProductDto>>(result.Error);

        var availability = await _availabilityRepository.GetAvailableQuantitiesAsync(
            result.Value.Select(product => product.ProductId).ToArray(),
            cancellationToken);

        return Result.Success(result.Value
            .Select(product => CatalogMappings.ToGetProductDto(
                product,
                availability.GetValueOrDefault(product.ProductId)))
            .ToList());
    }
}
