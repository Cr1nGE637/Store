using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Command;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CreateCategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, ICatalogUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateCategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryResult = Category.Create(request.CategoryName);
        if (categoryResult.IsFailure)
            return Result.Failure<CreateCategoryDto>(categoryResult.Error);

        await _categoryRepository.AddAsync(categoryResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(categoryResult.Value));
    }

    private static CreateCategoryDto MapToDto(Category c) => new(c.CategoryId, c.CategoryName);
}
