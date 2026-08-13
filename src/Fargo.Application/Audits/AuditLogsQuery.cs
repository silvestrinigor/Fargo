using Fargo.Application.Common;

namespace Fargo.Application.Audits;

public sealed record AuditLogsQuery(
    Pagination WithPagination
) : IQuery<IReadOnlyCollection<AuditLogDto>>;
