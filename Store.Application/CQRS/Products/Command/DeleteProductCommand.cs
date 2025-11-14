using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;

namespace Store.Application.CQRS.Products.Command;

public class DeleteProductCommand : IRequest<Result<GetProductDto>>
{
    public Guid ProductId { get; init; }
}