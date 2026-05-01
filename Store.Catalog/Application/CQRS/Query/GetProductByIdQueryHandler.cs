using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Query;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<GetProductDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<GetProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _productRepository.GetByIdAsync(request.ProductId);
        if (result.IsFailure)
            return Result.Failure<GetProductDto>(result.Error);

        return Result.Success(MapToDto(result.Value));
    }

    private static GetProductDto MapToDto(Product p) =>
        new(p.ProductId, p.ProductName, p.ProductDescription, p.ProductPrice, p.CategoryId);
}
