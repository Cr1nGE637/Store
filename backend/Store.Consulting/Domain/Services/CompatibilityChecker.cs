using System.Globalization;
using Store.Consulting.Domain.Entities;
using Store.Consulting.Domain.Enums;
using Store.Consulting.Domain.ValueObjects;

namespace Store.Consulting.Domain.Services;

public sealed class CompatibilityChecker
{
    public ConsultationResult Check(
        IReadOnlyCollection<ConsultationProduct> products,
        IReadOnlyCollection<CompatibilityRule> rules)
    {
        var issues = new List<CompatibilityIssue>();
        var recommendations = new List<ProductRecommendation>();

        foreach (var rule in rules.Where(r => r.IsActive))
        {
            var sourceProducts = products
                .Where(product => CategoryMatches(product, rule.SourceCategoryCode))
                .ToArray();

            var targetProducts = products
                .Where(product => CategoryMatches(product, rule.TargetCategoryCode))
                .ToArray();

            foreach (var sourceProduct in sourceProducts)
            foreach (var targetProduct in targetProducts)
            {
                if (sourceProduct.ProductId == targetProduct.ProductId)
                    continue;

                var issue = CheckPair(rule, sourceProduct, targetProduct);
                if (issue is null)
                    continue;

                issues.Add(issue);
                if (rule.RecommendationType.HasValue)
                {
                    recommendations.Add(new ProductRecommendation(
                        rule.RecommendationType.Value,
                        issue.Message,
                        sourceProduct.ProductId,
                        targetProduct.ProductId));
                }
            }
        }

        return new ConsultationResult(
            ResolveStatus(issues),
            issues,
            recommendations,
            DateTime.UtcNow);
    }

    private static CompatibilityIssue? CheckPair(
        CompatibilityRule rule,
        ConsultationProduct sourceProduct,
        ConsultationProduct targetProduct)
    {
        var hasSourceValue = TryGetSpecification(
            sourceProduct,
            rule.Condition.NormalizedSourceSpecificationKey,
            out var sourceValue);
        var hasTargetValue = TryGetSpecification(
            targetProduct,
            rule.Condition.NormalizedTargetSpecificationKey,
            out var targetValue);

        if (!hasSourceValue || !hasTargetValue)
        {
            return new CompatibilityIssue(
                CompatibilitySeverity.Unknown,
                rule.Code,
                FormatMessage(rule, sourceProduct, targetProduct, sourceValue, targetValue),
                sourceProduct.ProductId,
                targetProduct.ProductId);
        }

        var expected = rule.Condition.ExpectedValue;
        var comparisonValue = string.IsNullOrWhiteSpace(expected) ? targetValue : expected;
        if (Matches(sourceValue, comparisonValue, rule.Condition.Operator))
            return null;

        return new CompatibilityIssue(
            rule.Severity,
            rule.Code,
            FormatMessage(rule, sourceProduct, targetProduct, sourceValue, targetValue),
            sourceProduct.ProductId,
            targetProduct.ProductId);
    }

    private static bool TryGetSpecification(
        ConsultationProduct product,
        string key,
        out string value)
    {
        foreach (var specification in product.Specifications)
        {
            if (!string.Equals(
                    CompatibilityCondition.NormalizeSpecificationKey(specification.Key),
                    key,
                    StringComparison.OrdinalIgnoreCase))
                continue;

            value = specification.Value;
            return !string.IsNullOrWhiteSpace(value);
        }

        value = string.Empty;
        return false;
    }

    private static bool Matches(string sourceValue, string comparisonValue, CompatibilityOperator op) =>
        op switch
        {
            CompatibilityOperator.Equals => string.Equals(sourceValue, comparisonValue, StringComparison.OrdinalIgnoreCase),
            CompatibilityOperator.NotEquals => !string.Equals(sourceValue, comparisonValue, StringComparison.OrdinalIgnoreCase),
            CompatibilityOperator.In => SplitValues(comparisonValue)
                .Any(value => string.Equals(value, sourceValue, StringComparison.OrdinalIgnoreCase)),
            CompatibilityOperator.GreaterThanOrEqual => TryParseDecimal(sourceValue, out var sourceNumber)
                && TryParseDecimal(comparisonValue, out var comparisonNumber)
                && sourceNumber >= comparisonNumber,
            CompatibilityOperator.LessThanOrEqual => TryParseDecimal(sourceValue, out var sourceNumber)
                && TryParseDecimal(comparisonValue, out var comparisonNumber)
                && sourceNumber <= comparisonNumber,
            _ => false
        };

    private static string FormatMessage(
        CompatibilityRule rule,
        ConsultationProduct sourceProduct,
        ConsultationProduct targetProduct,
        string sourceValue,
        string targetValue) =>
        rule.MessageTemplate
            .Replace("{sourceProduct}", sourceProduct.ProductName, StringComparison.OrdinalIgnoreCase)
            .Replace("{targetProduct}", targetProduct.ProductName, StringComparison.OrdinalIgnoreCase)
            .Replace("{sourceValue}", string.IsNullOrWhiteSpace(sourceValue) ? "unknown" : sourceValue, StringComparison.OrdinalIgnoreCase)
            .Replace("{targetValue}", string.IsNullOrWhiteSpace(targetValue) ? "unknown" : targetValue, StringComparison.OrdinalIgnoreCase);

    private static ConsultationStatus ResolveStatus(IReadOnlyCollection<CompatibilityIssue> issues)
    {
        if (issues.Any(issue => issue.Severity == CompatibilitySeverity.Error))
            return ConsultationStatus.Error;

        if (issues.Any(issue => issue.Severity == CompatibilitySeverity.Warning))
            return ConsultationStatus.Warning;

        if (issues.Any(issue => issue.Severity == CompatibilitySeverity.Unknown))
            return ConsultationStatus.Unknown;

        return ConsultationStatus.Ok;
    }

    private static IReadOnlyCollection<string> SplitValues(string value) =>
        value.Split([',', ';', '|'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

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

    private static bool CategoryMatches(ConsultationProduct product, string categoryCode) =>
        string.Equals(product.CategoryCode, categoryCode, StringComparison.OrdinalIgnoreCase);
}
