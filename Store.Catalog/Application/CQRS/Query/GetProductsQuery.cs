using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Query;

public class GetProductsQuery : IRequest<Result<List<GetProductDto>>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
