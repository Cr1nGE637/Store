namespace Store.Carts.Application.Interfaces;

public interface ICheckoutOrderRecorder
{
    Task RecordOrderCreatedAsync(
        Guid checkoutId,
        Guid orderId,
        Guid customerId,
        CancellationToken cancellationToken);
}
