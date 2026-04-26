using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.ApiClients;

/// <summary>Query to retrieve a paged, optionally filtered list of API clients.</summary>
/// <param name="Pagination">Pagination parameters, or <see langword="null"/> for defaults.</param>
/// <param name="Search">An optional search term to filter by name.</param>
public sealed record ApiClientManyQuery(Pagination? Pagination = null, string? Search = null) : IQuery<IReadOnlyCollection<ApiClientInformation>>;

/// <summary>Handles <see cref="ApiClientManyQuery"/>.</summary>
public sealed class ApiClientManyQueryHandler(
    ActorService actorService,
    IApiClientQueryRepository apiClientQueryRepository,
    ICurrentUser currentUser
) : IQueryHandler<ApiClientManyQuery, IReadOnlyCollection<ApiClientInformation>>
{
    /// <summary>Returns a paged list of API clients visible to the current actor.</summary>
    /// <param name="query">The query with pagination and search parameters.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task<IReadOnlyCollection<ApiClientInformation>> Handle(ApiClientManyQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditApiClient);

        return await apiClientQueryRepository.GetManyInfo(query.Pagination, query.Search, cancellationToken);
    }
}
