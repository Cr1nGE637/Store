using CSharpFunctionalExtensions;
using Store.Consulting.Application.CQRS.Query;
using Store.Consulting.Application.Interfaces;
using Store.Consulting.Application.Services;
using Store.Consulting.Domain.ValueObjects;

namespace Store.Tests.Application;

public class GetProductRecommendationsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenSmartphoneNeedsCharger_ReturnsOnlyChargersWithEnoughPower()
    {
        var smartphoneId = Guid.NewGuid();
        var weakChargerId = Guid.NewGuid();
        var strongChargerId = Guid.NewGuid();
        var handler = new GetProductRecommendationsQueryHandler(
            new FakeConsultingProductReadRepository(
                Product(smartphoneId, "Samsung Galaxy S24", "Smartphones", new Dictionary<string, string>
                {
                    ["connectorType"] = "USB-C",
                    ["requiredChargerWatts"] = "25"
                }),
                Product(weakChargerId, "Compact USB-C Charger", "Chargers", new Dictionary<string, string>
                {
                    ["connectorType"] = "USB-C",
                    ["powerWatts"] = "15"
                }),
                Product(strongChargerId, "Baseus GaN5 Pro 65W", "Chargers", new Dictionary<string, string>
                {
                    ["connectorType"] = "USB-C",
                    ["powerWatts"] = "65"
                })),
            new DefaultCompatibilityRuleProvider());

        var result = await handler.Handle(
            new GetProductRecommendationsQuery { ProductId = smartphoneId, Limit = 10 },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value, recommendation => recommendation.ProductId == strongChargerId);
        Assert.DoesNotContain(result.Value, recommendation => recommendation.ProductId == weakChargerId);
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
}
