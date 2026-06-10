using CSharpFunctionalExtensions;
using MediatR;
using Store.Consulting.Application.DTOs;
using Store.Consulting.Application.Interfaces;
using Store.Consulting.Domain.Services;

namespace Store.Consulting.Application.CQRS.Query;

public sealed class CheckCartCompatibilityQueryHandler
    : IRequestHandler<CheckCartCompatibilityQuery, Result<ConsultationResultDto>>
{
    private readonly IConsultingProductReadRepository _productRepository;
    private readonly ICompatibilityRuleProvider _ruleProvider;
    private readonly IConsultationResultRepository _resultRepository;
    private readonly CompatibilityChecker _checker;

    public CheckCartCompatibilityQueryHandler(
        IConsultingProductReadRepository productRepository,
        ICompatibilityRuleProvider ruleProvider,
        IConsultationResultRepository resultRepository,
        CompatibilityChecker checker)
    {
        _productRepository = productRepository;
        _ruleProvider = ruleProvider;
        _resultRepository = resultRepository;
        _checker = checker;
    }

    public async Task<Result<ConsultationResultDto>> Handle(
        CheckCartCompatibilityQuery request,
        CancellationToken cancellationToken)
    {
        var productIds = request.ProductIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (productIds.Length == 0)
            return Result.Failure<ConsultationResultDto>("At least one product is required for compatibility check");

        var productsResult = await _productRepository.GetByIdsAsync(productIds, cancellationToken);
        if (productsResult.IsFailure)
            return Result.Failure<ConsultationResultDto>(productsResult.Error);

        var products = productsResult.Value;
        var foundProductIds = products.Select(product => product.ProductId).ToHashSet();
        var missingProductIds = productIds.Where(id => !foundProductIds.Contains(id)).ToArray();
        if (missingProductIds.Length > 0)
            return Result.Failure<ConsultationResultDto>(
                $"Products not found: {string.Join(", ", missingProductIds)}");

        var rulesResult = await _ruleProvider.GetActiveRulesAsync(cancellationToken);
        if (rulesResult.IsFailure)
            return Result.Failure<ConsultationResultDto>(rulesResult.Error);

        var result = _checker.Check(products, rulesResult.Value);
        var consultationId = Guid.NewGuid();
        var dto = new ConsultationResultDto(
            consultationId,
            result.Status.ToString(),
            products
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
            result.CheckedAtUtc);

        await _resultRepository.SaveAsync(consultationId, request.CustomerId, productIds, dto, cancellationToken);

        return Result.Success(dto);
    }
}
