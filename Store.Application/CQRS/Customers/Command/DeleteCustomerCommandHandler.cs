using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;
using Store.Domain.Interfaces;

namespace Store.Application.CQRS.Customers.Command;

public class DeleteCustomerCommandHandler :  IRequestHandler<DeleteCustomerCommand, Result<GetCustomerDto>>
{
    private readonly ICustomersRepository _customersRepository;
    private readonly IMapper _mapper;

    public DeleteCustomerCommandHandler(ICustomersRepository customersRepository, IMapper mapper)
    {
        _customersRepository = customersRepository;
        _mapper = mapper;
    }

    public async Task<Result<GetCustomerDto>> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var productResult = await _customersRepository.GetByIdAsync(request.CustomerId);
        if (productResult.IsFailure)
        {
            return Result.Failure<GetCustomerDto>("Customer not found");
        }
        var product = productResult.Value;
        await _customersRepository.DeleteAsync(product.Id);
        
        var productDto = _mapper.Map<GetCustomerDto>(product);
        return Result.Success(productDto);
    }
}