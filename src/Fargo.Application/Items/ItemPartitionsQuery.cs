using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Items;
using Fargo.Domain.Partitions;

namespace Fargo.Application.Items;

/// <summary>
/// Query used to retrieve the partitions that directly contain a specific item.
/// </summary>
/// <param name="ItemGuid">The unique identifier of the item.</param>
public sealed record ItemPartitionsQuery(Guid ItemGuid) : IQuery<IReadOnlyCollection<PartitionInformation>?>;

/// <summary>
/// Handles <see cref="ItemPartitionsQuery"/>.
/// </summary>
public sealed class ItemPartitionsQueryHandler(
    ActorService actorService,
    IItemRepository itemRepository,
    ICurrentUser currentUser
) : IQueryHandler<ItemPartitionsQuery, IReadOnlyCollection<PartitionInformation>?>
{
    /// <summary>
    /// Returns the partitions that directly contain the item, filtered by the current
    /// actor's partition accesses. Admins and system actors receive all partitions unfiltered.
    /// Returns <see langword="null"/> if the item does not exist.
    /// </summary>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor is not authenticated or inactive.
    /// </exception>
    public async Task<IReadOnlyCollection<PartitionInformation>?> Handle(
        ItemPartitionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        var filter = (actor.IsAdmin || actor.IsSystem) ? null : actor.PartitionAccesses;

        return await itemRepository.GetPartitions(query.ItemGuid, filter, cancellationToken);
    }
}
