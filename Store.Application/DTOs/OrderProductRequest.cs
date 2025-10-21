namespace Store.Application.DTOs;

public record OrderProductRequest(Guid ProductId, int ProductQuantity);