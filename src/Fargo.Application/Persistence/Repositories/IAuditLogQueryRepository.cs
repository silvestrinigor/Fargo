using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos.Audits;
using Fargo.Application.Queries.Audits;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Persistence.Repositories;

public interface IAuditLogQueryRepository
{
    /// <summary>
    /// Null when the audit log does not exist or is not accessible to the reader described by
    /// <paramref name="accessScope"/>.
    /// </summary>
    Task<AuditLogDto?> GetInfoByGuidAsync(
        Guid auditLogGuid,
        AuditLogAccessScope accessScope,
        CancellationToken cancellationToken = default
    );

    Task<PagedResult<AuditLogDto>> GetManyInfoOrderedByOccurredAtAsync(
        Pagination pagination,
        AuditLogAccessScope accessScope,
        Guid? actorGuid = null,
        ActorType? actorType = null,
        DateTimeOffset? periodStart = null,
        DateTimeOffset? periodEnd = null,
        CancellationToken cancellationToken = default
    );
}
