using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;
using Store.Domain.Entities;
using Store.Domain.Interfaces;

namespace Store.Application.CQRS.Products.Command;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<GetProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<GetProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product.IsFailure)
        {
            return Result.Failure<GetProductDto>("Product not found");
        }
        var updatedProductResult = Product.Create(request.ProductName, request.ProductDescription, request.ProductPrice, request.ProductId);
        var updatedProduct = await _productRepository.UpdateProduct(updatedProductResult.Value);
        
        var updatedProductDto = _mapper.Map<GetProductDto>(updatedProduct.Value);
        return Result.Success(updatedProductDto);
    }
}