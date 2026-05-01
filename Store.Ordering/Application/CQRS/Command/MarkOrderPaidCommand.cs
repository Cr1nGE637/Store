using CSharpFunctionalExtensions;
using MediatR;

namespace Store.Ordering.Application.CQRS.Command;

public record MarkOrderPaidCommand(Guid OrderId) : IRequest<Result>;
