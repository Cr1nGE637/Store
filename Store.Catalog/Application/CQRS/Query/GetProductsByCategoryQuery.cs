using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Query;

public record GetProductsByCategoryQuery(Guid CategoryId) : IRequest<Result<List<GetProductDto>>>;
