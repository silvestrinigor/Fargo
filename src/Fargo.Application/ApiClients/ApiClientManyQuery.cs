using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.ApiClients;

public sealed record ApiClientManyQuery(Pagination? Pagination = null, string? Search = null) : IQuery<IReadOnlyCollection<ApiClientInformation>>;

public sealed class ApiClientManyQueryHandler(
    ActorService actorService,
    IApiClientQueryRepository apiClientQueryRepository,
    ICurrentUser currentUser
) : IQueryHandler<ApiClientManyQuery, IReadOnlyCollection<ApiClientInformation>>
{
    public async Task<IReadOnlyCollection<ApiClientInformation>> Handle(ApiClientManyQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditApiClient);

        return await apiClientQueryRepository.GetManyInfo(query.Pagination, query.Search, cancellationToken);
    }
}
