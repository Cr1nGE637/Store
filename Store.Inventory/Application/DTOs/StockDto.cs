namespace Store.Inventory.Application.DTOs;

public record StockDto(Guid ProductId, int Quantity, int Reserved, int Available);
