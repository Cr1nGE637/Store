using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Command;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<GetCategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        ICatalogUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GetCategoryDto>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var existingResult = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (existingResult.IsFailure)
            return Result.Failure<GetCategoryDto>("Category not found");

        var hasProducts = await _productRepository.HasProductsByCategoryAsync(request.CategoryId);
        if (hasProducts)
            return Result.Failure<GetCategoryDto>("Cannot delete category with existing products");

        var deleteResult = await _categoryRepository.DeleteAsync(request.CategoryId);
        if (deleteResult.IsFailure)
            return Result.Failure<GetCategoryDto>(deleteResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(CatalogMappings.ToGetCategoryDto(existingResult.Value));
    }
}
