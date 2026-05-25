using CSharpFunctionalExtensions;
using MediatR;
using Store.Ordering.Application.Interfaces;
using Store.Ordering.Domain.Enums;
using Store.Ordering.Domain.Interfaces;

namespace Store.Ordering.Application.CQRS.Command;

public class MarkOrderPaidCommandHandler(
    IOrderRepository orderRepository,
    IOrderingUnitOfWork unitOfWork,
    IOrderingDomainEventOutbox outbox,
    IPaymentGateway paymentGateway) : IRequestHandler<MarkOrderPaidCommand, Result>
{
    public async Task<Result> Handle(MarkOrderPaidCommand request, CancellationToken cancellationToken)
    {
        var orderResult = await orderRepository.GetByIdAsync(request.OrderId);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Error);

        var order = orderResult.Value;
        if (order.CustomerId != request.CustomerId)
            return Result.Failure("Access denied");
        if (order.Status == OrderStatus.Paid)
            return Result.Success();
        if (order.Status == OrderStatus.AwaitingStock)
            return Result.Failure("Order is awaiting stock reservation");
        if (order.Status == OrderStatus.Rejected)
            return Result.Failure("Rejected orders cannot be paid");
        if (order.Status == OrderStatus.Cancelled)
            return Result.Failure("Отмененный заказ нельзя оплатить");

        var paymentResult = await paymentGateway.CaptureAsync(
            order.OrderId,
            order.CustomerId,
            order.PaymentMethod,
            order.TotalAmount,
            cancellationToken);
        if (paymentResult.IsFailure)
            return Result.Failure(paymentResult.Error);

        var payResult = order.MarkAsPaid(paymentResult.Value.PaidAmount, paymentResult.Value.TransactionId);
        if (payResult.IsFailure)
            return payResult;
        if (!payResult.Value)
            return Result.Success();

        var updateResult = await orderRepository.UpdateAsync(order);
        if (updateResult.IsFailure)
            return updateResult;

        await outbox.AddAsync(order.DomainEvents, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        order.ClearDomainEvents();

        return Result.Success();
    }
}
