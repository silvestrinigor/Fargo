using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos.Items;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Items;
using Fargo.Core.Events;

namespace Fargo.Application.Commands.Items;

public sealed record ItemEventCreateCommand(ItemEventCreateDto Create) : ICommand<Guid>;

/// <remarks>
/// Only valid for an event-tracked article; entity-tracked articles record stock through their
/// individual <see cref="Item"/> instances.
/// </remarks>
public sealed class ItemEventCreateCommandHandler(
    ActorResolver actorService,
    IArticleRepository articleRepository,
    IItemRepository itemRepository,
    IItemEventRepository itemEventRepository,
    ICurrentActor currentActor
) : ICommandHandler<ItemEventCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(ItemEventCreateCommand command, CancellationToken cancellationToken = default)
    {
        var create = command.Create;

        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.CreateItem);

        var article = await articleRepository.GetByGuidAsync(create.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, create.ArticleGuid);

        actor.ThrowIfAccessDenied(article);

        // Domain invariants (event-tracked article, positive quantity, container
        // types, movement origin/destination rules) are enforced by the factories.
        var itemEvent = create.EventType switch
        {
            ItemEventType.Moviment => await BuildMovimentAsync(create, actor, article, cancellationToken),
            ItemEventType.Creation => ItemEvent.NewCreationEvent(
                article, await ResolveContainerAsync(create.ItemContainerGuid, actor, cancellationToken), create.Quantity, create.OccuredAt),
            ItemEventType.Deletion => ItemEvent.NewDeletionEvent(
                article, await ResolveContainerAsync(create.ItemContainerGuid, actor, cancellationToken), create.Quantity, create.OccuredAt),
            _ => throw new FargoCoreException(
                $"Unsupported item event type '{create.EventType}'.", FargoErrorType.InvalidOperation),
        };

        itemEventRepository.Add(itemEvent);

        return itemEvent.Guid;
    }

    private async Task<ItemEvent> BuildMovimentAsync(ItemEventCreateDto create, SecurityPrincipal actor, Article article, CancellationToken cancellationToken)
    {
        var fromContainer = await ResolveContainerAsync(create.FromItemContainerGuid, actor, cancellationToken);
        var toContainer = await ResolveContainerAsync(create.ToItemContainerGuid, actor, cancellationToken);

        return ItemEvent.NewMovimentEvent(article, fromContainer, toContainer, create.Quantity, create.OccuredAt);
    }

    private async Task<Item?> ResolveContainerAsync(Guid? containerGuid, SecurityPrincipal actor, CancellationToken cancellationToken)
    {
        if (containerGuid is not { } guid)
        {
            return null;
        }

        var container = await itemRepository.GetByGuidAsync(guid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(container, guid);

        actor.ThrowIfAccessDenied(container);

        return container;
    }
}
