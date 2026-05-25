using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Query;

public record GetCategoryByIdQuery(Guid CategoryId) : IRequest<Result<GetCategoryDto>>;
