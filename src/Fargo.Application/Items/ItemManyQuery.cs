using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Items;

namespace Fargo.Application.Items;

/// <summary>
/// Query used to retrieve a paginated collection of <see cref="ItemInformation"/>
/// accessible to the current actor.
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
/// When <see langword="null"/>, a default pagination is applied.
/// </param>
/// <remarks>
/// This query respects authorization and partition-based access control rules,
/// ensuring that only items visible to the current actor are returned.
/// </remarks>
public sealed record ItemManyQuery(
        Guid? ArticleGuid = null,
        DateTimeOffset? AsOfDateTime = null,
        Pagination? Pagination = null,
        Guid? PartitionGuid = null,
        bool? NoPartition = null
        ) : IQuery<IReadOnlyCollection<ItemInformation>>;

/// <summary>
/// Handles <see cref="ItemManyQuery"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for:
/// <list type="bullet">
/// <item><description>Validating and retrieving the current actor.</description></item>
/// <item><description>Applying role-based access rules (admin/system vs regular actor).</description></item>
/// <item><description>Filtering items based on partition access.</description></item>
/// <item><description>Applying optional article filter, temporal (as-of), and pagination constraints.</description></item>
/// </list>
///
/// <para>
/// Actors with administrative or system privileges bypass partition filtering
/// and can access all items.
/// </para>
///
/// <para>
/// Regular actors can only access items that belong to at least one
/// partition they have access to.
/// </para>
///
/// <para>
/// When using temporal queries (<c>AsOfDateTime</c>), if an item belonged
/// to a partition that has since been deleted at the time of the request,
/// the following rules apply:
/// <list type="bullet">
/// <item>
/// <description>
/// Administrative and system actors can still access the historical data.
/// </description>
/// </item>
/// <item>
/// <description>
/// Regular actors will not have access to such items, as the partition
/// no longer exists in the current context, and the result will be excluded
/// from the returned collection.
/// </description>
/// </item>
/// </list>
/// </para>
/// </remarks>
public sealed class ItemManyQueryHandler(
        ActorService actorService,
        IItemRepository itemRepository,
        ICurrentUser currentUser
        ) : IQueryHandler<ItemManyQuery, IReadOnlyCollection<ItemInformation>>
{
    /// <summary>
    /// Executes the query to retrieve item information accessible
    /// to the current actor.
    /// </summary>
    /// <param name="query">
    /// The query containing optional article filter, temporal parameter,
    /// and pagination settings.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A read-only collection of <see cref="ItemInformation"/> objects
    /// visible to the current actor.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    /// <remarks>
    /// If the actor has administrative or system privileges, items are retrieved
    /// without partition filtering.
    ///
    /// Otherwise, only items belonging to partitions accessible to the actor
    /// are returned.
    ///
    /// Pagination defaults to <see cref="Pagination.FirstPage20Items"/> when not specified.
    /// </remarks>
    public async Task<IReadOnlyCollection<ItemInformation>> Handle(
            ItemManyQuery query,
            CancellationToken cancellationToken = default
            )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        if (actor.IsAdmin || actor.IsSystem)
        {
            if (query.NoPartition == true)
            {
                return await itemRepository.GetManyInfoWithNoPartition(
                        query.Pagination ?? Pagination.FirstPage20Items,
                        query.AsOfDateTime,
                        cancellationToken
                        );
            }

            if (query.PartitionGuid.HasValue)
            {
                return await itemRepository.GetManyInfoInPartitions(
                        query.Pagination ?? Pagination.FirstPage20Items,
                        [query.PartitionGuid.Value],
                        query.ArticleGuid,
                        query.AsOfDateTime,
                        cancellationToken
                        );
            }

            return await itemRepository.GetManyInfo(
                    query.Pagination ?? Pagination.FirstPage20Items,
                    query.ArticleGuid,
                    query.AsOfDateTime,
                    cancellationToken
                    );
        }
        else
        {
            if (query.NoPartition == true)
            {
                return await itemRepository.GetManyInfoWithNoPartition(
                        query.Pagination ?? Pagination.FirstPage20Items,
                        query.AsOfDateTime,
                        cancellationToken
                        );
            }

            if (query.PartitionGuid.HasValue && !actor.PartitionAccesses.Contains(query.PartitionGuid.Value))
            {
                return [];
            }

            var partitions = query.PartitionGuid.HasValue
                ? (IReadOnlyCollection<Guid>)[query.PartitionGuid.Value]
                : actor.PartitionAccesses;

            return await itemRepository.GetManyInfoInPartitions(
                    query.Pagination ?? Pagination.FirstPage20Items,
                    partitions,
                    query.ArticleGuid,
                    query.AsOfDateTime,
                    cancellationToken
                    );
        }
    }
}
