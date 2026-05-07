using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Query;

public record GetProductsByCategoryQuery(Guid CategoryId, int Page = 1, int PageSize = 50)
    : IRequest<Result<List<GetProductDto>>>;
