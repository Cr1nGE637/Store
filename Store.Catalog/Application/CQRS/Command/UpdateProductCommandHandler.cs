using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;
using Store.SharedKernel.Interfaces;

namespace Store.Catalog.Application.CQRS.Command;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<GetProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<GetProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var existingResult = await _productRepository.GetByIdAsync(request.ProductId);
        if (existingResult.IsFailure)
            return Result.Failure<GetProductDto>("Product not found");

        var updatedResult = Product.Create(request.ProductName, request.ProductDescription, request.ProductPrice, request.ProductId);
        if (updatedResult.IsFailure)
            return Result.Failure<GetProductDto>(updatedResult.Error);

        var savedResult = await _productRepository.UpdateAsync(updatedResult.Value);
        if (savedResult.IsFailure)
            return Result.Failure<GetProductDto>(savedResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<GetProductDto>(savedResult.Value));
    }
}
