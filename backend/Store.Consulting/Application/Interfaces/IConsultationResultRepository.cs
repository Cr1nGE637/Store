using Store.Consulting.Application.DTOs;

namespace Store.Consulting.Application.Interfaces;

public interface IConsultationResultRepository
{
    Task SaveAsync(
        Guid consultationId,
        Guid? customerId,
        IReadOnlyCollection<Guid> productIds,
        ConsultationResultDto result,
        CancellationToken cancellationToken);
}
