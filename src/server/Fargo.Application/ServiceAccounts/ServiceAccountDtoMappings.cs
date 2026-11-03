using Fargo.Core.ServiceAccounts;
using System.Linq.Expressions;

namespace Fargo.Application.ServiceAccounts;

public static class ServiceAccountDtoMappings
{
    /// <summary>
    /// A reusable projection translated to SQL by EF Core so service-account
    /// reads materialize straight into <see cref="ServiceAccountDto"/> without
    /// loading the aggregate. The value objects are cast to string via their
    /// registered value converters (the same form used elsewhere for EF-
    /// translatable projections).
    /// </summary>
    public static readonly Expression<Func<ServiceAccount, ServiceAccountDto>> Projection = account => new ServiceAccountDto(
        account.Guid,
        (string)account.ClientId,
        (string)account.Description,
        account.IsActive,
        account.Permissions,
        account.Partitions.Select(partition => partition.PartitionGuid).ToArray(),
        account.PartitionAccessGuids);
}
