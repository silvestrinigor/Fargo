using Fargo.Application.Common;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.ServiceAccounts;

/// <summary>Query for a single service account by id.</summary>
public sealed record ServiceAccountSingleQuery(
    Guid ServiceAccountGuid
) : IQuery<ServiceAccountDto?>;

public sealed class ServiceAccountSingleQueryHandler(
    ActorResolver actorService,
    IServiceAccountQueryRepository serviceAccountQueryRepository,
    ICurrentActor currentActor
) : IQueryHandler<ServiceAccountSingleQuery, ServiceAccountDto?>
{
    public async Task<ServiceAccountDto?> HandleAsync(ServiceAccountSingleQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ReadServiceAccount);

        return await serviceAccountQueryRepository.GetInfoByGuidAsync(
            query.ServiceAccountGuid,
            actor.PartitionAccessGuids,
            cancellationToken);
    }
}
