using CSharpFunctionalExtensions;
using MediatR;
using Store.Consulting.Application.DTOs;
using Store.Consulting.Application.Interfaces;
using Store.Consulting.Domain.Services;

namespace Store.Consulting.Application.CQRS.Query;

public sealed class TestCompatibilityRuleQueryHandler(
    IConsultingProductReadRepository productRepository,
    CompatibilityChecker checker)
    : IRequestHandler<TestCompatibilityRuleQuery, Result<ConsultationResultDto>>
{
    public async Task<Result<ConsultationResultDto>> Handle(
        TestCompatibilityRuleQuery request,
        CancellationToken cancellationToken)
    {
        if (request.SourceProductId == Guid.Empty || request.TargetProductId == Guid.Empty)
            return Result.Failure<ConsultationResultDto>("Two products are required for compatibility rule test");

        var ruleResult = CompatibilityRulePayloadMapper.ToDomain(request.Payload);
        if (ruleResult.IsFailure)
            return Result.Failure<ConsultationResultDto>(ruleResult.Error);

        var productsResult = await productRepository.GetByIdsAsync(
            [request.SourceProductId, request.TargetProductId],
            cancellationToken);
        if (productsResult.IsFailure)
            return Result.Failure<ConsultationResultDto>(productsResult.Error);

        if (productsResult.Value.Count != 2)
            return Result.Failure<ConsultationResultDto>("Products not found for compatibility rule test");

        var result = checker.Check(productsResult.Value, [ruleResult.Value]);
        return Result.Success(new ConsultationResultDto(
            Guid.NewGuid(),
            result.Status.ToString(),
            productsResult.Value
                .Select(product => new ConsultationItemDto(
                    product.ProductId,
                    product.CategoryCode,
                    product.Specifications))
                .ToArray(),
            result.Issues
                .Select(issue => new CompatibilityFindingDto(
                    issue.Severity.ToString(),
                    issue.RuleCode,
                    issue.Message))
                .ToArray(),
            result.Recommendations
                .Select(recommendation => new ProductRecommendationDto(
                    recommendation.TargetProductId ?? recommendation.SourceProductId ?? Guid.Empty,
                    recommendation.Type.ToString(),
                    recommendation.Reason))
                .ToArray(),
            result.CheckedAtUtc));
    }
}
