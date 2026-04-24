using Fargo.Application.Authentication;
using Fargo.Domain;

namespace Fargo.Application.Items;

/// <summary>
/// Query used to retrieve a single <see cref="ItemInformation"/> projection
/// accessible to the current actor.
/// </summary>
/// <param name="ItemGuid">
/// The unique identifier of the item to retrieve.
/// </param>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned result represents the state of the item
/// as it existed at the specified date and time.
/// </param>
/// <remarks>
/// This query respects authorization and partition-based access control rules,
/// ensuring that only items visible to the current actor are returned.
/// </remarks>
public sealed record ItemSingleQuery(
        Guid ItemGuid,
        DateTimeOffset? AsOfDateTime = null
        ) : IQuery<ItemInformation?>;

/// <summary>
/// Handles <see cref="ItemSingleQuery"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for:
/// <list type="bullet">
/// <item><description>Validating and retrieving the current actor.</description></item>
/// <item><description>Applying role-based access rules (admin/system vs regular actor).</description></item>
/// <item><description>Filtering the item based on partition access.</description></item>
/// <item><description>Applying optional temporal (as-of) constraints.</description></item>
/// </list>
///
/// <para>
/// Actors with administrative or system privileges bypass partition filtering
/// and can access any item.
/// </para>
///
/// <para>
/// Regular actors can access the item if it belongs to at least one
/// partition they have access to, or if the item has no partition (public).
/// </para>
///
/// <para>
/// When using temporal queries (<c>AsOfDateTime</c>), if the item belonged
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
/// Regular actors will not have access to the item, as the partition
/// no longer exists in the current context, and <see langword="null"/> is returned.
/// </description>
/// </item>
/// </list>
/// </para>
///
/// <para>
/// If the item does not exist or is not accessible to the current actor,
/// <see langword="null"/> is returned.
/// </para>
/// </remarks>
public sealed class ItemSingleQueryHandler(
        ActorService actorService,
        IItemQueryRepository itemRepository,
        ICurrentUser currentUser
        ) : IQueryHandler<ItemSingleQuery, ItemInformation?>
{
    /// <summary>
    /// Executes the query to retrieve a single item information projection
    /// accessible to the current actor.
    /// </summary>
    /// <param name="query">
    /// The query containing the item identifier and optional temporal parameter.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The <see cref="ItemInformation"/> visible to the current actor,
    /// or <see langword="null"/> if the item does not exist or is not accessible.
    /// </returns>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    /// <remarks>
    /// If the actor has administrative or system privileges, the item is retrieved
    /// without partition filtering.
    ///
    /// Otherwise, the item is only returned if it belongs to at least one
    /// partition accessible to the actor.
    /// </remarks>
    public async Task<ItemInformation?> Handle(
            ItemSingleQuery query,
            CancellationToken cancellationToken = default
            )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        if (actor.IsAdmin || actor.IsSystem)
        {
            var itemInformation = await itemRepository.GetInfoByGuid(
                    query.ItemGuid,
                    query.AsOfDateTime,
                    cancellationToken
                    );

            return itemInformation;
        }
        else
        {
            var itemInformation = await itemRepository.GetInfoByGuidPublicOrInPartitions(
                    query.ItemGuid,
                    actor.PartitionAccesses,
                    query.AsOfDateTime,
                    cancellationToken
                    );

            return itemInformation;
        }
    }
}
