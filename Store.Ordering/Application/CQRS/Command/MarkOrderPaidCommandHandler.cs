using CSharpFunctionalExtensions;
using MediatR;
using Store.Ordering.Application.Interfaces;
using Store.Ordering.Domain.Interfaces;

namespace Store.Ordering.Application.CQRS.Command;

public class MarkOrderPaidCommandHandler(
    IOrderRepository orderRepository,
    IOrderingUnitOfWork unitOfWork,
    IOrderingDomainEventOutbox outbox) : IRequestHandler<MarkOrderPaidCommand, Result>
{
    public async Task<Result> Handle(MarkOrderPaidCommand request, CancellationToken cancellationToken)
    {
        var orderResult = await orderRepository.GetByIdAsync(request.OrderId);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Error);

        var order = orderResult.Value;
        if (request.CustomerId.HasValue && order.CustomerId != request.CustomerId.Value)
            return Result.Failure("Access denied");

        var payResult = order.MarkAsPaid();
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
