using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Items;
using Fargo.Core.Items.Events;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Items;

/// <summary>
/// Handles <see cref="MoveItemEventCommand"/> by recording a movement event for
/// an event-tracked article.
/// </summary>
/// <remarks>
/// A movement event is only valid for an article whose
/// <see cref="Article.TrackingMode"/> is <see cref="ArticleTrackingMode.Event"/>.
/// Entity-tracked articles record movements through their individual
/// <see cref="Item"/> instances instead.
/// </remarks>
public sealed class MoveItemEventCommandHandler(
    ActorResolver actorService,
    IArticleRepository articleRepository,
    IItemRepository itemRepository,
    IItemEventRepository itemEventRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<MoveItemEventCommandHandler> logger
) : ICommandHandler<MoveItemEventCommand>
{
    /// <summary>
    /// Records the movement event described by the command.
    /// </summary>
    /// <param name="command">The command carrying the movement to record.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task HandleAsync(MoveItemEventCommand command, CancellationToken cancellationToken = default)
    {
        var create = command.Create;

        logger.MoveItemEventStarted(create.ArticleGuid, currentActor.Guid, currentActor.ActorType);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.CreateItem);

        var article = await articleRepository.GetByGuidAsync(create.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, create.ArticleGuid, EntityType.Article);

        actor.ThrowIfAccessDenied(article);

        Item? fromContainer = null;

        if (create.FromItemContainerGuid is { } fromGuid)
        {
            fromContainer = await itemRepository.GetByGuidAsync(fromGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(fromContainer, fromGuid, EntityType.Item);

            actor.ThrowIfAccessDenied(fromContainer);
        }

        Item? toContainer = null;

        if (create.ToItemContainerGuid is { } toGuid)
        {
            toContainer = await itemRepository.GetByGuidAsync(toGuid, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(toContainer, toGuid, EntityType.Item);

            actor.ThrowIfAccessDenied(toContainer);
        }

        // Domain invariants (event-tracked article, positive quantity, distinct
        // origin/destination, container-type items) are enforced by the factory.
        var itemEvent = ItemEvent.NewMovimentEvent(
            article,
            fromContainer,
            toContainer,
            create.Quantity,
            create.OccuredAt);

        // The movement is recorded as an event against the article. The audit is
        // recorded against the item event itself, while inheriting the partition
        // scope from the article the event concerns.
        var audit = AuditLog.CreateAuditLog(
            actor, itemEvent, article, AuditLogType.ItemEventRecorded);

        audit.Metadata.AddArticleGuid(article.Guid);
        audit.Metadata.AddPackQuantity(create.Quantity);

        itemEventRepository.Add(itemEvent);

        auditLogRepository.Add(audit);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.MoveItemEventCompleted(
            itemEvent.Guid, article.Guid, currentActor.Guid, currentActor.ActorType);
    }
}
