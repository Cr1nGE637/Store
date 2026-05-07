using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Query;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<List<GetProductDto>>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<List<GetProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var pagination = Pagination.Normalize(request.Page, request.PageSize);
        if (pagination.IsFailure)
            return Result.Failure<List<GetProductDto>>(pagination.Error);

        var result = await _productRepository.GetPageAsync(pagination.Value.Skip, pagination.Value.Take);
        if (result.IsFailure)
            return Result.Failure<List<GetProductDto>>(result.Error);

        return Result.Success(result.Value.Select(CatalogMappings.ToGetProductDto).ToList());
    }
}
