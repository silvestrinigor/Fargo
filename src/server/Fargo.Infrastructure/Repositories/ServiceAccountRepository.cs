using Fargo.Application.Common;
using Fargo.Application.ServiceAccounts;
using Fargo.Core.Informations;
using Fargo.Core.ServiceAccounts;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class ServiceAccountRepository(FargoDbContext context)
    : IServiceAccountRepository, IServiceAccountQueryRepository
{
    private readonly DbSet<ServiceAccount> serviceAccounts = context.ServiceAccounts;

    public Task<ServiceAccount?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default)
        => serviceAccounts
            .Include(account => account.Partitions)
            .SingleOrDefaultAsync(account => account.Guid == guid, cancellationToken);

    public Task<ServiceAccount?> GetByClientIdAsync(Nameid clientId, CancellationToken cancellationToken = default)
        => serviceAccounts
            .Include(account => account.Partitions)
            .SingleOrDefaultAsync(account => account.ClientId == clientId, cancellationToken);

    public async Task<IReadOnlyCollection<Guid>> GetAllActivePartitionAccessGuidsAsync(
        Guid serviceGuid,
        CancellationToken cancellationToken = default)
    {
        // Load the account's directly-granted partition accesses (inactive
        // accounts grant nothing), then expand downward through the partition
        // hierarchy so access to a partition implies access to all of its
        // descendants — mirroring the user partition-access resolution.
        var account = await serviceAccounts
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.Guid == serviceGuid, cancellationToken);

        if (account is null || !account.IsActive || account.PartitionAccessGuids.Count == 0)
        {
            return [];
        }

        var grantedPartitionGuids = account.PartitionAccessGuids.ToArray();

        FormattableString query = $"""
        WITH RECURSIVE partition_tree AS
        (
            -- Seed with the partitions granted directly to the service account.
            SELECT p.guid, p.parent_partition_guid
            FROM partitions p
            WHERE p.guid = ANY({grantedPartitionGuids})

            UNION

            -- Expand downward: every child of an already-included partition is
            -- also accessible.
            SELECT child.guid, child.parent_partition_guid
            FROM partitions AS child
            INNER JOIN partition_tree AS parent
                ON child.parent_partition_guid = parent.guid
        )
        SELECT DISTINCT guid AS partition_guid
        FROM partition_tree;
        """;

        var guids = await context.Database
            .SqlQuery<Guid>(query)
            .ToListAsync(cancellationToken);

        return guids;
    }

    public void Add(ServiceAccount serviceAccount) => serviceAccounts.Add(serviceAccount);

    public void Remove(ServiceAccount serviceAccount) => serviceAccounts.Remove(serviceAccount);

    public async Task<ServiceAccountDto?> GetInfoByGuidAsync(
        Guid guid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        var account = await ApplyPartitionFilter(
                serviceAccounts.AsNoTracking(),
                childOfAnyOfThesePartitions)
            .Where(a => a.Guid == guid)
            .Select(ServiceAccountDtoMappings.Projection)
            .SingleOrDefaultAsync(cancellationToken);

        return account;
    }

    public async Task<PagedResult<ServiceAccountDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyPartitionFilter(
            serviceAccounts.AsNoTracking(),
            childOfAnyOfThesePartitions);

        var totalCount = await query.CountAsync(cancellationToken);

        var accounts = await query
            .OrderBy(a => a.Guid)
            .WithPagination(pagination)
            .Select(ServiceAccountDtoMappings.Projection)
            .ToListAsync(cancellationToken);

        return new PagedResult<ServiceAccountDto>(
            accounts,
            totalCount,
            pagination.Page.Value,
            pagination.Limit.Value);
    }

    // Restricts the query to service accounts whose partition membership
    // intersects the actor's accessible partitions, mirroring how users are
    // filtered. A null partition set means no filter (unscoped read).
    private static IQueryable<ServiceAccount> ApplyPartitionFilter(
        IQueryable<ServiceAccount> query,
        IReadOnlyCollection<Guid>? partitionGuids)
        => query.WherePartitionAccessible(
            partitionGuids,
            account => account.Partitions,
            partition => partition.PartitionGuid);
}
