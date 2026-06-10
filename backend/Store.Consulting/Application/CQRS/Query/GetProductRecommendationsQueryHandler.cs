using CSharpFunctionalExtensions;
using System.Globalization;
using MediatR;
using Store.Consulting.Application.DTOs;
using Store.Consulting.Application.Interfaces;
using Store.Consulting.Domain.Entities;
using Store.Consulting.Domain.Enums;
using Store.Consulting.Domain.ValueObjects;

namespace Store.Consulting.Application.CQRS.Query;

public sealed class GetProductRecommendationsQueryHandler(
    IConsultingProductReadRepository productRepository,
    ICompatibilityRuleProvider ruleProvider)
    : IRequestHandler<GetProductRecommendationsQuery, Result<IReadOnlyCollection<ProductRecommendationDto>>>
{
    public async Task<Result<IReadOnlyCollection<ProductRecommendationDto>>> Handle(
        GetProductRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ProductId == Guid.Empty)
            return Result.Failure<IReadOnlyCollection<ProductRecommendationDto>>("Product id is required");

        var productResult = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (productResult.IsFailure)
            return Result.Failure<IReadOnlyCollection<ProductRecommendationDto>>(productResult.Error);

        var rulesResult = await ruleProvider.GetActiveRulesAsync(cancellationToken);
        if (rulesResult.IsFailure)
            return Result.Failure<IReadOnlyCollection<ProductRecommendationDto>>(rulesResult.Error);

        var recommendations = new List<ProductRecommendationDto>();
        foreach (var rule in rulesResult.Value)
        {
            if (!rule.RecommendationType.HasValue)
                continue;

            await AddRecommendationsAsync(productResult.Value, rule, recommendations, request.Limit, cancellationToken);
            if (recommendations.Count >= request.Limit)
                break;
        }

        return Result.Success<IReadOnlyCollection<ProductRecommendationDto>>(
            recommendations
                .GroupBy(recommendation => recommendation.ProductId)
                .Select(group => group.First())
                .Take(Math.Clamp(request.Limit, 1, 20))
                .ToArray());
    }

    private async Task AddRecommendationsAsync(
        ConsultationProduct product,
        CompatibilityRule rule,
        List<ProductRecommendationDto> recommendations,
        int limit,
        CancellationToken cancellationToken)
    {
        if (MatchesCategory(product, rule.SourceCategoryCode))
        {
            var candidatesResult = await productRepository.GetByCategoryCodeAsync(rule.TargetCategoryCode, cancellationToken);
            if (candidatesResult.IsSuccess)
                AddCompatibleCandidates(product, candidatesResult.Value, rule, recommendations, limit);
        }

        if (MatchesCategory(product, rule.TargetCategoryCode))
        {
            var candidatesResult = await productRepository.GetByCategoryCodeAsync(rule.SourceCategoryCode, cancellationToken);
            if (candidatesResult.IsSuccess)
                AddCompatibleCandidates(product, candidatesResult.Value, Reverse(rule), recommendations, limit);
        }
    }

    private static void AddCompatibleCandidates(
        ConsultationProduct source,
        IReadOnlyCollection<ConsultationProduct> candidates,
        CompatibilityRule rule,
        List<ProductRecommendationDto> recommendations,
        int limit)
    {
        foreach (var candidate in candidates.Where(candidate => candidate.ProductId != source.ProductId))
        {
            if (!IsCompatible(source, candidate, rule))
                continue;

            recommendations.Add(new ProductRecommendationDto(
                candidate.ProductId,
                (rule.RecommendationType ?? RecommendationType.Accessory).ToString(),
                $"{candidate.ProductName} подходит к выбранному товару по правилу \"{rule.Name}\"."));

            if (recommendations.Count >= limit)
                return;
        }
    }

    private static bool IsCompatible(ConsultationProduct source, ConsultationProduct target, CompatibilityRule rule)
    {
        if (!TryGetSpecification(source, rule.Condition.NormalizedSourceSpecificationKey, out var sourceValue))
            return false;

        var comparisonValue = rule.Condition.ExpectedValue;
        if (string.IsNullOrWhiteSpace(comparisonValue)
            && !TryGetSpecification(target, rule.Condition.NormalizedTargetSpecificationKey, out comparisonValue))
            return false;

        return rule.Condition.Operator switch
        {
            CompatibilityOperator.Equals => string.Equals(sourceValue, comparisonValue, StringComparison.OrdinalIgnoreCase),
            CompatibilityOperator.NotEquals => !string.Equals(sourceValue, comparisonValue, StringComparison.OrdinalIgnoreCase),
            CompatibilityOperator.In => comparisonValue
                .Split([',', ';', '|'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Any(value => string.Equals(value, sourceValue, StringComparison.OrdinalIgnoreCase)),
            CompatibilityOperator.GreaterThanOrEqual => TryParseDecimal(sourceValue, out var sourceNumber)
                && TryParseDecimal(comparisonValue, out var comparisonNumber)
                && sourceNumber >= comparisonNumber,
            CompatibilityOperator.LessThanOrEqual => TryParseDecimal(sourceValue, out var sourceNumber)
                && TryParseDecimal(comparisonValue, out var comparisonNumber)
                && sourceNumber <= comparisonNumber,
            _ => false
        };
    }

    private static bool TryGetSpecification(ConsultationProduct product, string key, out string value)
    {
        foreach (var specification in product.Specifications)
        {
            if (!string.Equals(CompatibilityCondition.NormalizeSpecificationKey(specification.Key), key, StringComparison.OrdinalIgnoreCase))
                continue;

            value = specification.Value;
            return !string.IsNullOrWhiteSpace(value);
        }

        value = string.Empty;
        return false;
    }

    private static CompatibilityRule Reverse(CompatibilityRule rule) =>
        CompatibilityRule.Reconstitute(
            rule.Id,
            rule.Code,
            rule.Name,
            rule.TargetCategoryCode,
            rule.SourceCategoryCode,
            new CompatibilityCondition(
                rule.Condition.TargetSpecificationKey,
                rule.Condition.SourceSpecificationKey,
                ReverseOperator(rule.Condition.Operator),
                rule.Condition.ExpectedValue),
            rule.Severity,
            rule.MessageTemplate,
            rule.RecommendationType,
            rule.IsActive).Value;

    private static CompatibilityOperator ReverseOperator(CompatibilityOperator op) =>
        op switch
        {
            CompatibilityOperator.GreaterThanOrEqual => CompatibilityOperator.LessThanOrEqual,
            CompatibilityOperator.LessThanOrEqual => CompatibilityOperator.GreaterThanOrEqual,
            _ => op
        };

    private static bool TryParseDecimal(string value, out decimal number)
    {
        var numericText = new string(value
            .Where(character => char.IsDigit(character) || character is '.' or ',')
            .Select(character => character == ',' ? '.' : character)
            .ToArray());

        return decimal.TryParse(
            numericText,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out number);
    }

    private static bool MatchesCategory(ConsultationProduct product, string categoryCode) =>
        string.Equals(product.CategoryCode, categoryCode, StringComparison.OrdinalIgnoreCase);
}
