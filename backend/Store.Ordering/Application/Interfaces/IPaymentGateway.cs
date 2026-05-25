using CSharpFunctionalExtensions;

namespace Store.Ordering.Application.Interfaces;

public record PaymentResult(decimal PaidAmount, string TransactionId);

public interface IPaymentGateway
{
    Task<Result<PaymentResult>> CaptureAsync(
        Guid orderId,
        Guid customerId,
        string paymentMethod,
        decimal amount,
        CancellationToken cancellationToken);
}
