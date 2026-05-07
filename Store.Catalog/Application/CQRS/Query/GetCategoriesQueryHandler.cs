using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;
using Store.Catalog.Domain.Entities;
using Store.Catalog.Domain.Interfaces;

namespace Store.Catalog.Application.CQRS.Query;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<GetCategoryDto>>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<List<GetCategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var pagination = Pagination.Normalize(request.Page, request.PageSize);
        if (pagination.IsFailure)
            return Result.Failure<List<GetCategoryDto>>(pagination.Error);

        var result = await _categoryRepository.GetPageAsync(pagination.Value.Skip, pagination.Value.Take);
        if (result.IsFailure)
            return Result.Failure<List<GetCategoryDto>>(result.Error);

        return Result.Success(result.Value.Select(CatalogMappings.ToGetCategoryDto).ToList());
    }
}
