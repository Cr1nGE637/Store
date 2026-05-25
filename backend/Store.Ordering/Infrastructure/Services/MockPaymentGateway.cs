using CSharpFunctionalExtensions;
using Store.Ordering.Application.Interfaces;

namespace Store.Ordering.Infrastructure.Services;

public class MockPaymentGateway : IPaymentGateway
{
    public Task<Result<PaymentResult>> CaptureAsync(
        Guid orderId,
        Guid customerId,
        string paymentMethod,
        decimal amount,
        CancellationToken cancellationToken)
    {
        if (amount <= 0)
            return Task.FromResult(Result.Failure<PaymentResult>("Payment amount must be positive"));

        return Task.FromResult(Result.Success(new PaymentResult(amount, $"mock-{orderId:N}")));
    }
}
