namespace Store.Carts.Application.Interfaces;

public interface ICheckoutOrderLookup
{
    Task<bool> HasOrderForCheckoutAsync(Guid checkoutId, CancellationToken cancellationToken);
}
