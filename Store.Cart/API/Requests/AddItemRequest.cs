namespace Store.Carts.API.Requests;

public record AddItemRequest(Guid ProductId, int Quantity);
