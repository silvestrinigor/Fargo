using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.ApiClients;

public sealed record ApiClientSingleQuery(Guid Guid) : IQuery<ApiClientInformation?>;

public sealed class ApiClientSingleQueryHandler(
    ActorService actorService,
    IApiClientQueryRepository apiClientQueryRepository,
    ICurrentUser currentUser
) : IQueryHandler<ApiClientSingleQuery, ApiClientInformation?>
{
    public async Task<ApiClientInformation?> Handle(ApiClientSingleQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditApiClient);

        return await apiClientQueryRepository.GetInfoByGuid(query.Guid, cancellationToken);
    }
}
