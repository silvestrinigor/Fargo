using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.ItemQueries;

/// <summary>
/// Query used to retrieve a paginated collection of item information
/// accessible to the current user.
/// </summary>
/// <param name="ArticleGuid">
/// Optional filter used to retrieve only items associated with a specific article.
/// When provided, only items belonging to the specified article are returned.
/// </param>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned results represent the state of the items
/// as they existed at the specified date and time.
/// </param>
/// <param name="Pagination">
/// Optional pagination configuration used to control the number of returned
/// results and the starting position of the query.
/// When <see langword="null"/>, a default pagination is used.
/// </param>
public sealed record ItemManyQuery(
        Guid? ArticleGuid = null,
        DateTimeOffset? AsOfDateTime = null,
        Pagination? Pagination = null
        ) : IQuery<IReadOnlyCollection<ItemInformation>>;

/// <summary>
/// Handles the execution of <see cref="ItemManyQuery"/>.
/// </summary>
/// <remarks>
/// This handler retrieves the current active user, resolves all partitions
/// the user can access including descendant partitions, and then returns
/// only items that belong to at least one of those partitions.
///
/// If the current user has no accessible partitions, the repository returns
/// an empty result set.
/// </remarks>
public sealed class ItemManyQueryHandler(
        ActorService actorService,
        IItemRepository itemRepository,
        ICurrentUser currentUser
        ) : IQueryHandler<ItemManyQuery, IReadOnlyCollection<ItemInformation>>
{
    /// <summary>
    /// Executes the query to retrieve item information accessible
    /// to the current user.
    /// </summary>
    /// <param name="query">
    /// The query containing the optional article filter, as-of date,
    /// and pagination settings.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="ItemInformation"/> objects
    /// accessible to the current user.
    /// </returns>
    public async Task<IReadOnlyCollection<ItemInformation>> Handle(
            ItemManyQuery query,
            CancellationToken cancellationToken = default
            )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedUserActorByGuid(currentUser.UserGuid, cancellationToken);

        var items = await itemRepository.GetManyInfoInPartitions(
                query.Pagination ?? Pagination.FirstPage20Items,
                actor.PartitionAccesses,
                query.ArticleGuid,
                query.AsOfDateTime,
                cancellationToken
                );

        return items;
    }
}
