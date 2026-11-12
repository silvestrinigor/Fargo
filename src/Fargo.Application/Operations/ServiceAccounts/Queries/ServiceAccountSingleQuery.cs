using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Operations.ServiceAccounts.Dtos;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Operations.ServiceAccounts;

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
