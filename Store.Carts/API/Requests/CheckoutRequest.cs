namespace Store.Carts.API.Requests;

public record CheckoutRequest(
    string RecipientName,
    string Phone,
    string DeliveryAddress,
    string DeliveryMethod,
    string PaymentMethod);
