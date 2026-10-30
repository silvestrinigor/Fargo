using Fargo.Application.Common;
using Fargo.Core.Actors;

namespace Fargo.Application.ServiceAccounts;

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

        return await serviceAccountQueryRepository.GetInfoByGuidAsync(query.ServiceAccountGuid, cancellationToken);
    }
}
