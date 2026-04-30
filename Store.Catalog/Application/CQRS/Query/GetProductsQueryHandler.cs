using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Query;

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
        var result = await _productRepository.GetAllAsync();
        if (result.IsFailure)
            return Result.Failure<List<GetProductDto>>(result.Error);

        return Result.Success(_mapper.Map<List<GetProductDto>>(result.Value));
    }
}
