using Fargo.Application.Common;
using Fargo.Application.Shared.Audits;

namespace Fargo.Application.Audits;

public sealed record AuditLogsQuery(
    Pagination WithPagination
) : IQuery<IReadOnlyCollection<AuditLogDto>>;
