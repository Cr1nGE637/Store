using AutoMapper;
using CSharpFunctionalExtensions;
using MediatR;
using Store.Application.DTOs;
using Store.Domain.Aggregates;
using Store.Domain.Interfaces;
using Store.Domain.ValueObjects;

namespace Store.Application.CQRS.Orders.Command;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<CreateOrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = Order.Create(request.CustomerId);
        if (order.IsFailure)
        {
            return Result.Failure<CreateOrderDto>(order.Error);
        }
        var productResult = await _productRepository.GetByIdAsync(request.Products.First().ProductId);
        if (productResult.IsFailure) return Result.Failure<CreateOrderDto>(productResult.Error);
        var product = productResult.Value;
        foreach (var orderProduct in request.Products)
        {
            var orderProductResult = OrderedProduct.Create(
                orderProduct.ProductId,
                orderProduct.ProductQuantity,
                product.Name,
                product.Price
            );

            if (orderProductResult.IsFailure)
            {
                return Result.Failure<CreateOrderDto>(orderProductResult.Error);
            }
            
            order.Value.AddProduct(orderProductResult.Value);
        }
        var result = await _orderRepository.CreateAsync(order.Value);
        return new Result<CreateOrderDto>();
    }
}