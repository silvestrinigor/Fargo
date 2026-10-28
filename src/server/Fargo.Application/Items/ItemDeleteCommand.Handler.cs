using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Items;

namespace Fargo.Application.Items;

public sealed class ItemDeleteCommandHandler(
    ActorResolver actorService,
    IItemRepository itemRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor
) : ICommandHandler<ItemDeleteCommand>
{
    public async Task HandleAsync(
        ItemDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.DeleteItem);

        var item = await itemRepository.GetByGuidAsync(command.ItemGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(item, command.ItemGuid, EntityType.Item);

        actor.ThrowIfAccessDenied(item);

        var audit = AuditLog.CreateAuditLog(actor, item, AuditLogType.EntityDeleted);

        auditLogRepository.Add(audit);

        itemRepository.Remove(item);
    }
}
