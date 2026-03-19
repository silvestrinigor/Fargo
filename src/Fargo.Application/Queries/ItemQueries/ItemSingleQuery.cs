using Fargo.Application.Extensions;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.ItemQueries;

/// <summary>
/// Query used to retrieve a single item information projection
/// accessible to the current user.
/// </summary>
/// <param name="ItemGuid">
/// The unique identifier of the item to retrieve.
/// </param>
/// <param name="AsOfDateTime">
/// Optional point in time used to retrieve historical data.
/// When provided, the returned result represents the state of the item
/// as it existed at the specified date and time.
/// </param>
public sealed record ItemSingleQuery(
    Guid ItemGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ItemInformation?>;

/// <summary>
/// Handles the execution of <see cref="ItemSingleQuery"/>.
/// </summary>
/// <remarks>
/// This handler retrieves the current active user, resolves all partitions
/// the user can access including descendant partitions, and then returns
/// the requested item only if it belongs to at least one of those partitions.
///
/// If the item does not exist or is not accessible to the current user,
/// <see langword="null"/> is returned.
/// </remarks>
public sealed class ItemSingleQueryHandler(
    IItemRepository itemRepository,
    IUserRepository userRepository,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser
) : IQueryHandler<ItemSingleQuery, ItemInformation?>
{
    /// <summary>
    /// Executes the query to retrieve a single item information projection
    /// accessible to the current user.
    /// </summary>
    /// <param name="query">
    /// The query containing the item identifier and optional as-of date.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The <see cref="ItemInformation"/> accessible to the current user,
    /// or <see langword="null"/> if the item does not exist or is not accessible.
    /// </returns>
    public async Task<ItemInformation?> Handle(
        ItemSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(query);

        var actor = await userRepository.GetActiveCurrentUser(currentUser, cancellationToken);

        var partitionAccessGuids = await partitionRepository.GetDescendantGuids(
            [.. actor.PartitionAccesses.Select(x => x.PartitionGuid)],
            includeRoots: true,
            cancellationToken
        );

        var itemInformation = await itemRepository.GetInfoByGuidInPartitions(
            query.ItemGuid,
            partitionAccessGuids,
            query.AsOfDateTime,
            cancellationToken
        );

        return itemInformation;
    }
}
