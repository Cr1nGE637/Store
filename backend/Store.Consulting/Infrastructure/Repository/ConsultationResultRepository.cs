using System.Text.Json;
using Store.Consulting.Application.DTOs;
using Store.Consulting.Application.Interfaces;
using Store.Consulting.Infrastructure.DbContexts;
using Store.Consulting.Infrastructure.Entity;

namespace Store.Consulting.Infrastructure.Repository;

public sealed class ConsultationResultRepository(ConsultingDbContext dbContext) : IConsultationResultRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task SaveAsync(
        Guid consultationId,
        Guid? customerId,
        IReadOnlyCollection<Guid> productIds,
        ConsultationResultDto result,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        await dbContext.ConsultationResults.AddAsync(new ConsultationResultEntity
        {
            Id = consultationId,
            CustomerId = customerId,
            Status = result.Status,
            ProductIdsJson = JsonSerializer.Serialize(productIds, JsonOptions),
            ItemsJson = JsonSerializer.Serialize(result.Items, JsonOptions),
            FindingsJson = JsonSerializer.Serialize(result.Findings, JsonOptions),
            RecommendationsJson = JsonSerializer.Serialize(result.Recommendations, JsonOptions),
            CheckedAtUtc = result.CheckedAtUtc,
            CreatedAtUtc = now
        }, cancellationToken);

        foreach (var recommendation in result.Recommendations)
        {
            await dbContext.RecommendationEvents.AddAsync(new RecommendationEventEntity
            {
                Id = Guid.NewGuid(),
                ConsultationId = consultationId,
                ProductId = recommendation.ProductId,
                Type = recommendation.Type,
                Reason = recommendation.Reason,
                CreatedAtUtc = now
            }, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
