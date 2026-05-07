using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var existing = await _categoryRepository.GetByNameAsync(request.CategoryName);
        if (existing.IsSuccess)
            return Result.Failure<CreateCategoryDto>("Category already exists");

        var categoryResult = Category.Create(request.CategoryName);
        if (categoryResult.IsFailure)
            return Result.Failure<CreateCategoryDto>(categoryResult.Error);

        await _categoryRepository.AddAsync(categoryResult.Value);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (CatalogPersistenceErrors.IsUniqueViolation(ex))
        {
            return Result.Failure<CreateCategoryDto>("Category already exists");
        }

        return Result.Success(CatalogMappings.ToCreateCategoryDto(categoryResult.Value));
    }
}
