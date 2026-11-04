using Fargo.Core.ServiceAccounts;
using System.Linq.Expressions;

namespace Fargo.Application.ServiceAccounts;

public static class ServiceAccountDtoMappings
{
    /// <summary>
    /// A reusable projection translated to SQL by EF Core so service-account
    /// reads materialize straight into <see cref="ServiceAccountDto"/> without
    /// loading the aggregate. The shared authorization state is read from the
    /// owning actor. The value objects are cast to string via their registered
    /// value converters (the same form used elsewhere for EF-translatable
    /// projections).
    /// </summary>
    public static readonly Expression<Func<ServiceAccount, ServiceAccountDto>> Projection = account => new ServiceAccountDto(
        account.ActorGuid,
        (string)account.Actor.Nameid,
        (string)account.Actor.Description,
        account.Actor.IsActive,
        account.Actor.Permissions,
        account.Actor.Partitions.Select(partition => partition.PartitionGuid).ToArray(),
        account.Actor.PartitionAccesses.Select(access => access.PartitionGuid).ToArray());
}
