using CSharpFunctionalExtensions;
using MediatR;
using Store.Ordering.Application.Interfaces;
using Store.Ordering.Domain.Interfaces;

namespace Store.Ordering.Application.CQRS.Command;

public class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IOrderingUnitOfWork unitOfWork,
    IPublisher publisher) : IRequestHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var orderResult = await orderRepository.GetByIdAsync(request.OrderId);
        if (orderResult.IsFailure)
            return Result.Failure(orderResult.Error);

        var order = orderResult.Value;
        var cancelResult = order.Cancel();
        if (cancelResult.IsFailure)
            return cancelResult;

        await orderRepository.UpdateAsync(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in order.DomainEvents)
            await publisher.Publish(domainEvent, cancellationToken);

        return Result.Success();
    }
}
