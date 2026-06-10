using CSharpFunctionalExtensions;
using Store.Consulting.Domain.ValueObjects;

namespace Store.Consulting.Application.Interfaces;

public interface IConsultingProductReadRepository
{
    Task<Result<ConsultationProduct>> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyCollection<ConsultationProduct>>> GetByIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyCollection<ConsultationProduct>>> GetByCategoryCodeAsync(
        string categoryCode,
        CancellationToken cancellationToken);
}
