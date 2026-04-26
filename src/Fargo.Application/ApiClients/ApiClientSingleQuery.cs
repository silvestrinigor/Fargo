using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.ApiClients;

/// <summary>Query to retrieve a single API client by its unique identifier.</summary>
/// <param name="Guid">The unique identifier of the API client.</param>
public sealed record ApiClientSingleQuery(Guid Guid) : IQuery<ApiClientInformation?>;

/// <summary>Handles <see cref="ApiClientSingleQuery"/>.</summary>
public sealed class ApiClientSingleQueryHandler(
    ActorService actorService,
    IApiClientQueryRepository apiClientQueryRepository,
    ICurrentUser currentUser
) : IQueryHandler<ApiClientSingleQuery, ApiClientInformation?>
{
    /// <summary>Returns the API client information, or <see langword="null"/> if not found.</summary>
    /// <param name="query">The query containing the API client identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task<ApiClientInformation?> Handle(ApiClientSingleQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditApiClient);

        return await apiClientQueryRepository.GetInfoByGuid(query.Guid, cancellationToken);
    }
}
