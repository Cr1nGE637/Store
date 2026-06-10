using CSharpFunctionalExtensions;
using Store.Consulting.Application.DTOs;
using Store.Consulting.Domain.Entities;
using Store.Consulting.Domain.Enums;

namespace Store.Consulting.Application;

internal static class CompatibilityRulePayloadMapper
{
    public static Result<CompatibilityRule> ToDomain(CompatibilityRulePayloadDto payload)
    {
        if (!Enum.TryParse<CompatibilityOperator>(payload.Operator, true, out var op))
            return Result.Failure<CompatibilityRule>("Compatibility operator is invalid");

        if (!Enum.TryParse<CompatibilitySeverity>(payload.Severity, true, out var severity))
            return Result.Failure<CompatibilityRule>("Compatibility severity is invalid");

        RecommendationType? recommendationType = null;
        if (!string.IsNullOrWhiteSpace(payload.RecommendationType))
        {
            if (!Enum.TryParse<RecommendationType>(payload.RecommendationType, true, out var parsedRecommendationType))
                return Result.Failure<CompatibilityRule>("Recommendation type is invalid");

            recommendationType = parsedRecommendationType;
        }

        return CompatibilityRule.Create(
            payload.Code,
            payload.Name,
            payload.SourceCategoryCode,
            payload.TargetCategoryCode,
            new CompatibilityCondition(
                payload.SourceSpecificationKey,
                payload.TargetSpecificationKey,
                op,
                payload.ExpectedValue),
            severity,
            payload.MessageTemplate,
            recommendationType,
            payload.IsActive);
    }
}
