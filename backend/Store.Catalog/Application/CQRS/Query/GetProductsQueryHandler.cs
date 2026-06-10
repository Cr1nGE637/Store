using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;
using Store.Catalog.Domain.ValueObjects;

namespace Store.Catalog.Application.CQRS.Query;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<List<GetProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductAvailabilityRepository _availabilityRepository;
    private readonly IProductImageRepository _imageRepository;

    public GetProductsQueryHandler(
        IProductRepository productRepository,
        IProductAvailabilityRepository availabilityRepository,
        IProductImageRepository imageRepository)
    {
        _productRepository = productRepository;
        _availabilityRepository = availabilityRepository;
        _imageRepository = imageRepository;
    }

    public async Task<Result<List<GetProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var pagination = Pagination.Normalize(request.Page, request.PageSize);
        if (pagination.IsFailure)
            return Result.Failure<List<GetProductDto>>(pagination.Error);

        if (request.MinPrice.HasValue && request.MaxPrice.HasValue && request.MinPrice > request.MaxPrice)
            return Result.Failure<List<GetProductDto>>("Minimum price cannot be greater than maximum price");

        var specificationFilters = NormalizeSpecificationFilters(request.SpecificationFilters);
        var criteria = new ProductSearchCriteria(
            request.Search,
            request.CategoryId,
            request.Brand,
            request.MinPrice,
            request.MaxPrice,
            specificationFilters,
            request.InStockOnly,
            pagination.Value.Skip,
            pagination.Value.Take);

        var result = await _productRepository.SearchAsync(criteria);
        if (result.IsFailure)
            return Result.Failure<List<GetProductDto>>(result.Error);

        var availability = await _availabilityRepository.GetAvailableQuantitiesAsync(
            result.Value.Select(product => product.ProductId).ToArray(),
            cancellationToken);
        var mainImages = await _imageRepository.GetMainImagesAsync(
            result.Value.Select(product => product.ProductId).ToArray(),
            cancellationToken);

        return Result.Success(result.Value
            .Select(product => CatalogMappings.ToGetProductDto(
                product,
                availability.GetValueOrDefault(product.ProductId),
                mainImages.GetValueOrDefault(product.ProductId)))
            .ToList());
    }

    private static IReadOnlyDictionary<string, string> NormalizeSpecificationFilters(
        IReadOnlyDictionary<string, string>? filters)
    {
        if (filters is null || filters.Count == 0)
            return new Dictionary<string, string>();

        var normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in filters)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
                continue;

            normalized[ProductSpecification.NormalizeName(key)] = value.Trim();
        }

        return normalized;
    }
}
