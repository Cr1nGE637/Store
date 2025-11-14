using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;
using Store.Domain.Entities;
using Store.Domain.Interfaces;
using Store.Domain.ValueObjects;

namespace Store.Application.CQRS.Customers.Command;

public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, Result<GetCustomerDto>>
{
    private readonly ICustomersRepository _customersRepository;
    private readonly IMapper _mapper;

    public UpdateCustomerCommandHandler(ICustomersRepository customersRepository, IMapper mapper)
    {
        _customersRepository = customersRepository;
        _mapper = mapper;
    }

    public async Task<Result<GetCustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var existingCustomer = await _customersRepository.GetByIdAsync(request.Id);
        if (existingCustomer.IsFailure)
        {
            return Result.Failure<GetCustomerDto>("Customer not found");
        }
        
        var email = Email.Create(request.Email);
        if (email.IsFailure)
        {
            return Result.Failure<GetCustomerDto>("Email not valid");
        }
        
        var customer = Customer.Create(request.Name, email.Value, request.Id);
        var customerResult = await _customersRepository.UpdateAsync(customer.Value);
        if (customerResult.IsFailure)
        {
            return Result.Failure<GetCustomerDto>(customerResult.Error);
        }
        
        var updatedCustomer = customerResult.Value;
        var updatedCustomerDto = _mapper.Map<GetCustomerDto>(updatedCustomer);
        return Result.Success(updatedCustomerDto);
    }
}