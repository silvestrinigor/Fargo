using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Audits;
using Fargo.Core.Common;
using Fargo.Core.Entities;
using Fargo.Core.Items;
using Fargo.Core.Items.Events;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

/// <summary>
/// Handles <see cref="ItemEventCreateCommand"/> by recording a movement,
/// creation, or deletion event for an event-tracked article.
/// </summary>
/// <remarks>
/// Item events are only valid for an article whose
/// <see cref="Article.TrackingMode"/> is <see cref="ArticleTrackingMode.Event"/>.
/// Entity-tracked articles record stock through their individual
/// <see cref="Item"/> instances instead.
/// </remarks>
public sealed class ItemEventCreateCommandHandler(
    ActorResolver actorService,
    IArticleRepository articleRepository,
    IItemRepository itemRepository,
    IItemEventRepository itemEventRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<ItemEventCreateCommandHandler> logger
) : ICommandHandler<ItemEventCreateCommand, Guid>
{
    /// <summary>
    /// Records the item event described by the command and returns its identifier.
    /// </summary>
    /// <param name="command">The command carrying the event to record.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The identifier of the recorded <see cref="ItemEvent"/>.</returns>
    public async Task<Guid> HandleAsync(ItemEventCreateCommand command, CancellationToken cancellationToken = default)
    {
        var create = command.Create;

        logger.ItemEventCreateStarted(create.EventType, create.ArticleGuid, currentActor.Guid, currentActor.ActorType);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.CreateItem);

        var article = await articleRepository.GetByGuidAsync(create.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, create.ArticleGuid, EntityType.Article);

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

        // The event is recorded against the article. The audit is recorded against
        // the item event itself, inheriting the partition scope from the article.
        var audit = AuditLog.CreateAuditLog(actor, itemEvent, article, AuditLogType.ItemEventRecorded);

        audit.Metadata.AddArticleGuid(article.Guid);
        audit.Metadata.AddPackQuantity(create.Quantity);

        itemEventRepository.Add(itemEvent);

        auditLogRepository.Add(audit);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.ItemEventCreateCompleted(itemEvent.Guid, create.EventType, article.Guid, currentActor.Guid, currentActor.ActorType);

        return itemEvent.Guid;
    }

    private async Task<ItemEvent> BuildMovimentAsync(ItemEventCreateDto create, Actor actor, Article article, CancellationToken cancellationToken)
    {
        var fromContainer = await ResolveContainerAsync(create.FromItemContainerGuid, actor, cancellationToken);
        var toContainer = await ResolveContainerAsync(create.ToItemContainerGuid, actor, cancellationToken);

        return ItemEvent.NewMovimentEvent(article, fromContainer, toContainer, create.Quantity, create.OccuredAt);
    }

    private async Task<Item?> ResolveContainerAsync(Guid? containerGuid, Actor actor, CancellationToken cancellationToken)
    {
        if (containerGuid is not { } guid)
        {
            return null;
        }

        var container = await itemRepository.GetByGuidAsync(guid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(container, guid, EntityType.Item);

        actor.ThrowIfAccessDenied(container);

        return container;
    }
}
