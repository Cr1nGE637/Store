using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;
using Store.Domain.Entities;
using Store.Domain.Interfaces;
using Store.Domain.ValueObjects;

namespace Store.Application.CQRS.Customers.Command;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CreateCustomerDto>>
{
    private readonly ICustomersRepository _customersRepository;
    private readonly IMapper _mapper;

    public CreateCustomerCommandHandler(ICustomersRepository customersRepository, IMapper mapper)
    {
        _customersRepository = customersRepository;
        _mapper = mapper;
    }

    public async Task<Result<CreateCustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var existingCustomer = await _customersRepository.GetByEmailAsync(request.Email);
        if (existingCustomer.IsSuccess)
        {
            return Result.Failure<CreateCustomerDto>("Customer already exists");
        }
        
        var email = Email.Create(request.Email);
        if (email.IsFailure)
        {
            return Result.Failure<CreateCustomerDto>(email.Error);
        }
        
        var customerResult = Customer.Create(request.Name, email.Value);
        if (customerResult.IsFailure)
        {
            return Result.Failure<CreateCustomerDto>(customerResult.Error);
        }
        
        var customer = customerResult.Value;
        await _customersRepository.AddAsync(customer);
        var customerDto = _mapper.Map<CreateCustomerDto>(customer);
        return Result.Success(customerDto);
    }
}