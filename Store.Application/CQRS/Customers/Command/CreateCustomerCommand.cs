using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;
using Store.Domain.ValueObjects;

namespace Store.Application.CQRS.Customers.Command;

public class CreateCustomerCommand : IRequest<Result<CreateCustomerDto>>
{
    public string Name { get; init; }
    public string Email { get; init; }
}