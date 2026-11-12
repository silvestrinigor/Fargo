using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.ServiceAccounts;
using System.Linq.Expressions;

namespace Fargo.Application.Operations.ServiceAccounts.Dtos;

/// <summary>Never exposes the secret.</summary>
public sealed record ServiceAccountDto(
    Guid Guid,
    string ClientId,
    string Description,
    bool IsActive,
    IReadOnlyCollection<ActionType> Permissions,
    IReadOnlyCollection<Guid> PartitionGuids,
    IReadOnlyCollection<Guid> PartitionAccessGuids
);

public static class ServiceAccountDtoMappings
{
    /// <summary>
    /// Translated to SQL by EF Core, so reads materialize without loading the aggregate. The value
    /// objects are cast to string through their registered value converters.
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
