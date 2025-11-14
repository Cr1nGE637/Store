using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;

namespace Store.Application.CQRS.Products.Command;

public class UpdateProductCommand : IRequest<Result<GetProductDto>>
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductDescription { get; init; } = string.Empty;
    public decimal ProductPrice { get; init; }
}