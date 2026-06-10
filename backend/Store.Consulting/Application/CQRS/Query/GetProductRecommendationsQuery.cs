using CSharpFunctionalExtensions;
using MediatR;
using Store.Consulting.Application.DTOs;

namespace Store.Consulting.Application.CQRS.Query;

public sealed class GetProductRecommendationsQuery : IRequest<Result<IReadOnlyCollection<ProductRecommendationDto>>>
{
    public Guid ProductId { get; init; }
    public int Limit { get; init; } = 6;
}
