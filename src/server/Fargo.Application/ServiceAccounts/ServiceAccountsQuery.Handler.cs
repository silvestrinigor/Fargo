using Fargo.Application.Common;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.ServiceAccounts;

public sealed class ServiceAccountsQueryHandler(
    ActorResolver actorService,
    IServiceAccountQueryRepository serviceAccountQueryRepository,
    ICurrentActor currentActor
) : IQueryHandler<ServiceAccountsQuery, PagedResult<ServiceAccountDto>>
{
    public async Task<PagedResult<ServiceAccountDto>> HandleAsync(ServiceAccountsQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ReadServiceAccount);

        var partitionGuids = PartitionQueryFilter.ForPartitionedEntities(
            actor.PartitionAccessGuids,
            requestedPartitionGuids: null);

        return await serviceAccountQueryRepository.GetManyInfoAsync(
            query.WithPagination,
            partitionGuids,
            cancellationToken);
    }
}
