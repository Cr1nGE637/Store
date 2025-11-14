namespace Store.Application.DTOs;

public record CreateOrderDto(Guid CustomerId, List<OrderProductDto> Products);