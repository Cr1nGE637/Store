using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

        var duplicateName = await _categoryRepository.GetByNameAsync(request.CategoryName);
        if (duplicateName.IsSuccess && duplicateName.Value.CategoryId != request.CategoryId)
            return Result.Failure<GetCategoryDto>("Category already exists");

        var category = existingResult.Value;
        var updateResult = category.Update(request.CategoryName);
        if (updateResult.IsFailure)
            return Result.Failure<GetCategoryDto>(updateResult.Error);

        var saveResult = await _categoryRepository.UpdateAsync(category);
        if (saveResult.IsFailure)
            return Result.Failure<GetCategoryDto>(saveResult.Error);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (CatalogPersistenceErrors.IsUniqueViolation(ex))
        {
            return Result.Failure<GetCategoryDto>("Category already exists");
        }

        return Result.Success(CatalogMappings.ToGetCategoryDto(category));
    }
}
