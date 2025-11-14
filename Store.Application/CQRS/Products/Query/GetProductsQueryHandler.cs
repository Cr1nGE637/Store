using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;
using Store.Domain.Interfaces;

namespace Store.Application.CQRS.Products.Query;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<List<GetProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    
    public GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<GetProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    { 
        var products = await _productRepository.GetAllAsync(); 
        var productDtos = _mapper.Map<List<GetProductDto>>(products.Value);
        return Result.Success(productDtos);
    }
}