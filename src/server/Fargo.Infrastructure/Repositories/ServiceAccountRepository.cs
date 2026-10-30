using Fargo.Application.Common;
using Fargo.Application.ServiceAccounts;
using Fargo.Core.Informations;
using Fargo.Core.ServiceAccounts;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories;

public sealed class ServiceAccountRepository(FargoDbContext context)
    : IServiceAccountRepository, IServiceAccountQueryRepository
{
    private readonly DbSet<ServiceAccount> serviceAccounts = context.ServiceAccounts;

    public Task<ServiceAccount?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default)
        => serviceAccounts.SingleOrDefaultAsync(account => account.Guid == guid, cancellationToken);

    public Task<ServiceAccount?> GetByClientIdAsync(Nameid clientId, CancellationToken cancellationToken = default)
        => serviceAccounts.SingleOrDefaultAsync(account => account.ClientId == clientId, cancellationToken);

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

    public async Task<ServiceAccountDto?> GetInfoByGuidAsync(Guid guid, CancellationToken cancellationToken = default)
    {
        var account = await serviceAccounts
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.Guid == guid, cancellationToken);

        return account?.ToDto();
    }

    public async Task<PagedResult<ServiceAccountDto>> GetManyInfoAsync(
        Pagination pagination,
        CancellationToken cancellationToken = default)
    {
        var query = serviceAccounts.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var accounts = await query
            .OrderBy(a => a.Guid)
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .ToListAsync(cancellationToken);

        return new PagedResult<ServiceAccountDto>(
            [.. accounts.Select(a => a.ToDto())],
            totalCount,
            pagination.Page.Value,
            pagination.Limit.Value);
    }
}
