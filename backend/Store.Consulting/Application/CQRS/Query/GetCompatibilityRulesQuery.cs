using CSharpFunctionalExtensions;
using MediatR;
using Store.Consulting.Application.DTOs;

namespace Store.Consulting.Application.CQRS.Query;

public sealed class GetCompatibilityRulesQuery : IRequest<Result<IReadOnlyCollection<CompatibilityRuleDto>>>;
