using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;

namespace Store.Application.CQRS.Customers.Command;

public class UpdateCustomerCommand : IRequest<Result<GetCustomerDto>>
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string Name { get; init; }
}