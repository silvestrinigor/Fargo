using Fargo.Application.Common;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.UserGroups;

namespace Fargo.Application.UserGroups;

public sealed class UserGroupDeleteCommandHandler(
    ActorResolver actorService,
    UserGroupService userGroupService,
    IUserGroupRepository userGroupRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserGroupDeleteCommand>
{
    public async Task HandleAsync(
        UserGroupDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.DeleteUserGroup);

        var userGroup = await userGroupRepository.GetByGuidAsync(command.UserGroupGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(userGroup, command.UserGroupGuid, EntityType.UserGroup);

        actor.ThrowIfAccessDenied(userGroup);

        await userGroupService.ValidateUserGroupCanBeDeletedAsync(userGroup, cancellationToken);

        userGroupRepository.Remove(userGroup);

        var audit = AuditLog.CreateAuditLog(actor, userGroup, AuditLogType.EntityDeleted);

        auditLogRepository.Add(audit);
    }
}
