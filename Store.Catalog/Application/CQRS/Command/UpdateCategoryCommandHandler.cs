using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Application.Interfaces;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Command;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<GetCategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, ICatalogUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GetCategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var existingResult = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (existingResult.IsFailure)
            return Result.Failure<GetCategoryDto>(existingResult.Error);

        var category = existingResult.Value;
        var updateResult = category.Update(request.CategoryName);
        if (updateResult.IsFailure)
            return Result.Failure<GetCategoryDto>(updateResult.Error);

        var savedResult = await _categoryRepository.UpdateAsync(category);
        if (savedResult.IsFailure)
            return Result.Failure<GetCategoryDto>(savedResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(savedResult.Value));
    }

    private static GetCategoryDto MapToDto(Category c) => new(c.CategoryId, c.CategoryName);
}
