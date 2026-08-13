using Fargo.Application.Audits;
using Fargo.Application.Common;
using Fargo.Core.Audits;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class AuditLogRepository(FargoDbContext context) : IAuditLogRepository, IAuditLogQueryRepository
{
    public void Add(AuditLog auditLog)
    {
        context.AuditLogs.Add(auditLog);
    }

    public async Task<IReadOnlyCollection<AuditLogDto>> GetManyInfoAsync(Pagination pagination, IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null, CancellationToken cancellationToken = default)
    {
        var queryFiltered = ApplyPartitionFilter(
            context.AuditLogs.AsNoTracking(),
            childOfAnyOfThesePartitions);

        return await queryFiltered
            .OrderBy(auditlog => auditlog.Guid)
            .WithPagination(pagination)
            .Select(AuditLogDtoMappings.Projection)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<AuditLog> ApplyPartitionFilter(
        IQueryable<AuditLog> query,
        IReadOnlyCollection<Guid>? partitionGuids)
    {
        if (partitionGuids is null)
        {
            return query;
        }

        return query.Where(auditLog =>
            auditLog.Partitions.Any(partition => partitionGuids.Contains(partition.PartitionGuid)));
    }
}
