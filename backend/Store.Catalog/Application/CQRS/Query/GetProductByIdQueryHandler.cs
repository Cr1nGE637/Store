using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Query;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<GetProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductAvailabilityRepository _availabilityRepository;
    private readonly IProductImageRepository _imageRepository;

    public GetProductByIdQueryHandler(
        IProductRepository productRepository,
        IProductAvailabilityRepository availabilityRepository,
        IProductImageRepository imageRepository)
    {
        _productRepository = productRepository;
        _availabilityRepository = availabilityRepository;
        _imageRepository = imageRepository;
    }

    public async Task<Result<GetProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _productRepository.GetByIdAsync(request.ProductId);
        if (result.IsFailure)
            return Result.Failure<GetProductDto>(result.Error);

        var availability = await _availabilityRepository.GetAvailableQuantitiesAsync(
            [result.Value.ProductId],
            cancellationToken);
        var mainImages = await _imageRepository.GetMainImagesAsync(
            [result.Value.ProductId],
            cancellationToken);

        return Result.Success(CatalogMappings.ToGetProductDto(
            result.Value,
            availability.GetValueOrDefault(result.Value.ProductId),
            mainImages.GetValueOrDefault(result.Value.ProductId)));
    }
}
