using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Query;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<GetCategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<GetCategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (result.IsFailure)
            return Result.Failure<GetCategoryDto>(result.Error);

        return Result.Success(MapToDto(result.Value));
    }

    private static GetCategoryDto MapToDto(Category c) => new(c.CategoryId, c.CategoryName);
}
