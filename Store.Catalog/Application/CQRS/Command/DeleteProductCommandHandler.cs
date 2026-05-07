using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Command;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<GetProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;
    private readonly ICatalogDomainEventOutbox _outbox;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        ICatalogUnitOfWork unitOfWork,
        ICatalogDomainEventOutbox outbox)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _outbox = outbox;
    }

    public async Task<Result<GetProductDto>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var productResult = await _productRepository.GetByIdAsync(request.ProductId);
        if (productResult.IsFailure)
            return Result.Failure<GetProductDto>("Product not found");

        var product = productResult.Value;
        product.Delete();

        var deleteResult = await _productRepository.DeleteAsync(product.ProductId);
        if (deleteResult.IsFailure)
            return Result.Failure<GetProductDto>(deleteResult.Error);

        await _outbox.AddAsync(product.DomainEvents, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        product.ClearDomainEvents();

        return Result.Success(CatalogMappings.ToGetProductDto(product));
    }
}
