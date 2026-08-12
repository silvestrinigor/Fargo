using Fargo.Application.Shared.Audits;

namespace Fargo.Application.Audits;

public interface IAuditLogQueryRepository
{
    Task<IReadOnlyCollection<AuditLogDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );
}
