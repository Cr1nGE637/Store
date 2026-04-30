using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;
using Store.SharedKernel.Interfaces;

namespace Store.Catalog.Application.CQRS.Command;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<CreateProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CreateProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existing = await _productRepository.GetByNameAsync(request.ProductName);
        if (existing != null)
            return Result.Failure<CreateProductDto>("Product already exists");

        var productResult = Product.Create(request.ProductName, request.ProductDescription, request.ProductPrice);
        if (productResult.IsFailure)
            return Result.Failure<CreateProductDto>(productResult.Error);

        await _productRepository.AddAsync(productResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<CreateProductDto>(productResult.Value));
    }
}
