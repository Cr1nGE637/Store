using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Query;

public record GetProductByIdQuery(Guid ProductId) : IRequest<Result<GetProductDto>>;
