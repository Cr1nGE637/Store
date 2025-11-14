using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;
using Store.Domain.Interfaces;

namespace Store.Application.CQRS.Products.Command;

public class DeleteProductCommandHandler :  IRequestHandler<DeleteProductCommand, Result<GetProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public DeleteProductCommandHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<GetProductDto>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var productResult = await _productRepository.GetByIdAsync(request.ProductId);
        if (productResult.IsFailure)
        {
            return Result.Failure<GetProductDto>("Product not found");
        }
        var product = productResult.Value;
        await _productRepository.Delete(product.Id);
        return Result.Success(_mapper.Map<GetProductDto>(product));
    }
}