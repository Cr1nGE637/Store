using CSharpFunctionalExtensions;
using MediatR;
using Store.Catalog.Application.DTOs;

namespace Store.Catalog.Application.CQRS.Command;

public class DeleteProductCommand : IRequest<Result<GetProductDto>>
{
    public Guid ProductId { get; init; }
}
