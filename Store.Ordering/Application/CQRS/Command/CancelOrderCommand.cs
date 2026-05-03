using CSharpFunctionalExtensions;
using MediatR;

namespace Store.Ordering.Application.CQRS.Command;

public record CancelOrderCommand(Guid OrderId, Guid RequesterId, bool IsManager) : IRequest<Result>;
