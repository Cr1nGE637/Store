using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;

namespace Store.Application.CQRS.Customers.Command;

public class DeleteCustomerCommand : IRequest<Result<GetCustomerDto>>
{
    public Guid CustomerId { get; init; }
}