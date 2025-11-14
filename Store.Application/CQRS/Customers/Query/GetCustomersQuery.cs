using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;

namespace Store.Application.CQRS.Customers.Query;

public class GetCustomersQuery : IRequest<Result<List<GetCustomerDto>>>
{
    
}