using Store.Ordering.Application.DTOs;

namespace Store.Ordering.Application;

internal static class OrderingMappings
{
    internal static GetOrderDto ToGetOrderDto(Domain.Aggregates.Order order) => new(
        order.OrderId,
        order.CustomerId,
        order.RecipientName,
        order.Phone,
        order.PaymentMethod,
        order.TotalAmount,
        order.Status.ToString(),
        order.CreatedAt,
        order.PaidAt,
        order.PaidAmount,
        order.PaymentTransactionId,
        order.CancelledAt,
        order.Products.Select(p => new OrderedProductDto(p.ProductId, p.ProductName, p.Price, p.Quantity)).ToList());
}
