using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Query;

public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, Result<List<GetProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public GetProductsByCategoryQueryHandler(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<List<GetProductDto>>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var categoryResult = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (categoryResult.IsFailure)
            return Result.Failure<List<GetProductDto>>("Category not found");

        var productsResult = await _productRepository.GetByCategoryIdAsync(request.CategoryId);
        if (productsResult.IsFailure)
            return Result.Failure<List<GetProductDto>>(productsResult.Error);

        return Result.Success(productsResult.Value.Select(CatalogMappings.ToGetProductDto).ToList());
    }
}
