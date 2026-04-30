using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Domain.Interfaces;
using Store.SharedKernel.Interfaces;

namespace Store.Catalog.Application.CQRS.Command;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<GetProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<GetProductDto>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var productResult = await _productRepository.GetByIdAsync(request.ProductId);
        if (productResult.IsFailure)
            return Result.Failure<GetProductDto>("Product not found");

        var deleteResult = await _productRepository.DeleteAsync(productResult.Value.ProductId);
        if (deleteResult.IsFailure)
            return Result.Failure<GetProductDto>(deleteResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<GetProductDto>(productResult.Value));
    }
}
