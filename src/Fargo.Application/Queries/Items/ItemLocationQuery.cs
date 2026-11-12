using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Dtos.Items;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Queries.Items;

/// <summary>
/// The chain of items containing the queried item.
/// </summary>
/// <param name="At">
/// UTC instant to evaluate the location at; each link uses the containment valid then, not the
/// current one. <see langword="null"/> returns the current location.
/// </param>
public sealed record ItemLocationQuery(
    Guid ItemGuid,
    DateTimeOffset? At = null
) : IQuery<IReadOnlyCollection<ItemDto>>;

public sealed class ItemLocationQueryHandler(
    ActorResolver actorService,
    IItemQueryRepository itemRepository,
    ICurrentActor currentActor
) : IQueryHandler<ItemLocationQuery, IReadOnlyCollection<ItemDto>>
{
    public async Task<IReadOnlyCollection<ItemDto>> HandleAsync(
        ItemLocationQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var item = query.At is { } at
            ? await itemRepository.GetLocationInfoByGuidAtInstantOrderedByDepthAsync(
                query.ItemGuid,
                at,
                actor.PartitionAccessGuids,
                cancellationToken)
            : await itemRepository.GetLocationInfoByGuidOrderedByDepthAsync(
                query.ItemGuid,
                actor.PartitionAccessGuids,
                cancellationToken);

        return item;
    }
}
