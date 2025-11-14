using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;
using Store.Domain.Entities;
using Store.Domain.Interfaces;

namespace Store.Application.CQRS.Products.Command;

public class CreateProductCommandHandler :  IRequestHandler<CreateProductCommand, Result<CreateProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    
    public CreateProductCommandHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }
    
    public async Task<Result<CreateProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _productRepository.GetByNameAsync(request.ProductName);
        if (existingProduct != null) 
            return Result.Failure<CreateProductDto>("Product is already exists");
        
        var productResult = Product.Create(request.ProductName, request.ProductDescription, request.ProductPrice);
        if (productResult.IsFailure)
            return Result.Failure<CreateProductDto>(productResult.Error);
        
        var product = productResult.Value;
        await _productRepository.AddAsync(product);
        
        var dto = _mapper.Map<CreateProductDto>(product);
        return Result.Success(dto);
    }
}