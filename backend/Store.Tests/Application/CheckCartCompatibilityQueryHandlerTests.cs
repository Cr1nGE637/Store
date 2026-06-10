using CSharpFunctionalExtensions;
using Store.Consulting.Application.CQRS.Query;
using Store.Consulting.Application.DTOs;
using Store.Consulting.Application.Interfaces;
using Store.Consulting.Application.Services;
using Store.Consulting.Domain.Services;
using Store.Consulting.Domain.ValueObjects;

namespace Store.Tests.Application;

public class CheckCartCompatibilityQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenCartContainsCompatibleParts_ReturnsOk()
    {
        var cpuId = Guid.NewGuid();
        var motherboardId = Guid.NewGuid();
        var handler = new CheckCartCompatibilityQueryHandler(
            new FakeConsultingProductReadRepository(
                Product(cpuId, "AMD Ryzen 5 7600", "Processors", new Dictionary<string, string> { ["socket"] = "AM5" }),
                Product(motherboardId, "ASUS B650-Plus", "Motherboards", new Dictionary<string, string> { ["socket"] = "AM5" })),
            new DefaultCompatibilityRuleProvider(),
            new FakeConsultationResultRepository(),
            new CompatibilityChecker());

        var result = await handler.Handle(
            new CheckCartCompatibilityQuery { ProductIds = [cpuId, motherboardId] },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Ok", result.Value.Status);
        Assert.Empty(result.Value.Findings);
        Assert.Equal(2, result.Value.Items.Count);
    }

    [Fact]
    public async Task Handle_WhenCartContainsIncompatibleParts_ReturnsExplainableError()
    {
        var cpuId = Guid.NewGuid();
        var motherboardId = Guid.NewGuid();
        var handler = new CheckCartCompatibilityQueryHandler(
            new FakeConsultingProductReadRepository(
                Product(cpuId, "Intel Core i5-13400F", "Processors", new Dictionary<string, string> { ["socket"] = "LGA1700" }),
                Product(motherboardId, "ASUS B650-Plus", "Motherboards", new Dictionary<string, string> { ["socket"] = "AM5" })),
            new DefaultCompatibilityRuleProvider(),
            new FakeConsultationResultRepository(),
            new CompatibilityChecker());

        var result = await handler.Handle(
            new CheckCartCompatibilityQuery { ProductIds = [cpuId, motherboardId] },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Error", result.Value.Status);
        var finding = Assert.Single(result.Value.Findings);
        Assert.Equal("cpu_motherboard_socket", finding.RuleCode);
        Assert.Contains("LGA1700", finding.Message);
        Assert.Single(result.Value.Recommendations);
    }

    [Fact]
    public async Task Handle_WhenCartHasMissingSpecification_ReturnsUnknown()
    {
        var cpuId = Guid.NewGuid();
        var motherboardId = Guid.NewGuid();
        var handler = new CheckCartCompatibilityQueryHandler(
            new FakeConsultingProductReadRepository(
                Product(cpuId, "AMD Ryzen 5 7600", "Processors", new Dictionary<string, string> { ["socket"] = "AM5" }),
                Product(motherboardId, "ASUS B650-Plus", "Motherboards", new Dictionary<string, string>())),
            new DefaultCompatibilityRuleProvider(),
            new FakeConsultationResultRepository(),
            new CompatibilityChecker());

        var result = await handler.Handle(
            new CheckCartCompatibilityQuery { ProductIds = [cpuId, motherboardId] },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Unknown", result.Value.Status);
        var finding = Assert.Single(result.Value.Findings);
        Assert.Equal("Unknown", finding.Severity);
        Assert.Contains("unknown", finding.Message);
    }

    [Fact]
    public async Task Handle_WhenChargerPowerIsBelowSmartphoneRequirement_ReturnsWarning()
    {
        var chargerId = Guid.NewGuid();
        var smartphoneId = Guid.NewGuid();
        var handler = new CheckCartCompatibilityQueryHandler(
            new FakeConsultingProductReadRepository(
                Product(chargerId, "Compact USB-C Charger", "Chargers", new Dictionary<string, string>
                {
                    ["connectorType"] = "USB-C",
                    ["powerWatts"] = "15"
                }),
                Product(smartphoneId, "Samsung Galaxy S24", "Smartphones", new Dictionary<string, string>
                {
                    ["connectorType"] = "USB-C",
                    ["requiredChargerWatts"] = "25"
                })),
            new DefaultCompatibilityRuleProvider(),
            new FakeConsultationResultRepository(),
            new CompatibilityChecker());

        var result = await handler.Handle(
            new CheckCartCompatibilityQuery { ProductIds = [chargerId, smartphoneId] },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Warning", result.Value.Status);
        var finding = Assert.Single(result.Value.Findings);
        Assert.Equal("smartphone_charger_power", finding.RuleCode);
        Assert.Contains("15", finding.Message);
        Assert.Contains("25", finding.Message);
    }

    [Fact]
    public async Task Handle_WhenProductIdsAreEmpty_ReturnsFailure()
    {
        var handler = new CheckCartCompatibilityQueryHandler(
            new FakeConsultingProductReadRepository(),
            new DefaultCompatibilityRuleProvider(),
            new FakeConsultationResultRepository(),
            new CompatibilityChecker());

        var result = await handler.Handle(new CheckCartCompatibilityQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("At least one product is required for compatibility check", result.Error);
    }

    [Fact]
    public async Task Handle_WhenProductIsMissing_ReturnsFailure()
    {
        var missingId = Guid.NewGuid();
        var handler = new CheckCartCompatibilityQueryHandler(
            new FakeConsultingProductReadRepository(),
            new DefaultCompatibilityRuleProvider(),
            new FakeConsultationResultRepository(),
            new CompatibilityChecker());

        var result = await handler.Handle(
            new CheckCartCompatibilityQuery { ProductIds = [missingId] },
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Contains(missingId.ToString(), result.Error);
    }

    private static ConsultationProduct Product(
        Guid id,
        string name,
        string categoryCode,
        IReadOnlyDictionary<string, string> specifications) =>
        new(id, name, categoryCode, specifications);

    private sealed class FakeConsultingProductReadRepository(params ConsultationProduct[] products)
        : IConsultingProductReadRepository
    {
        private readonly IReadOnlyCollection<ConsultationProduct> _products = products;

        public Task<Result<ConsultationProduct>> GetByIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            var product = _products.FirstOrDefault(item => item.ProductId == productId);
            return Task.FromResult(product is null
                ? Result.Failure<ConsultationProduct>("Product not found")
                : Result.Success(product));
        }

        public Task<Result<IReadOnlyCollection<ConsultationProduct>>> GetByIdsAsync(
            IReadOnlyCollection<Guid> productIds,
            CancellationToken cancellationToken)
        {
            var requested = productIds.ToHashSet();
            var found = _products
                .Where(product => requested.Contains(product.ProductId))
                .ToArray();

            return Task.FromResult(Result.Success<IReadOnlyCollection<ConsultationProduct>>(found));
        }

        public Task<Result<IReadOnlyCollection<ConsultationProduct>>> GetByCategoryCodeAsync(
            string categoryCode,
            CancellationToken cancellationToken)
        {
            var found = _products
                .Where(product => string.Equals(product.CategoryCode, categoryCode, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            return Task.FromResult(Result.Success<IReadOnlyCollection<ConsultationProduct>>(found));
        }
    }

    private sealed class FakeConsultationResultRepository : IConsultationResultRepository
    {
        public Task SaveAsync(
            Guid consultationId,
            Guid? customerId,
            IReadOnlyCollection<Guid> productIds,
            ConsultationResultDto result,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
