using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;

namespace Store.Application.CQRS.Orders.Command;

public class CreateOrderCommand : IRequest<Result<CreateOrderDto>>
{
    public Guid CustomerId { get; init; }
    public List<OrderProductDto> Products { get; init; } = [];
}