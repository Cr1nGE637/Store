using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;
using Store.Domain.Interfaces;

namespace Store.Application.CQRS.Customers.Query;

public class GetCustomerQueryHandler : IRequestHandler<GetCustomersQuery, Result<List<GetCustomerDto>>>
{
    private readonly ICustomersRepository _customersRepository;
    private readonly IMapper _mapper;

    public GetCustomerQueryHandler(ICustomersRepository customersRepository, IMapper mapper)
    {
        _customersRepository = customersRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<GetCustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _customersRepository.GetAllAsync(); 
        var customerDtos = _mapper.Map<List<GetCustomerDto>>(customers.Value);
        return Result.Success(customerDtos);
    }
}