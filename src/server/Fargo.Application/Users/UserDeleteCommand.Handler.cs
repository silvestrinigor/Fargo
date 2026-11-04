using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Users;

namespace Fargo.Application.Users;

public sealed class UserDeleteCommandHandler(
    ActorResolver actorService,
    IUserRepository userRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserDeleteCommand>
{
    public async Task HandleAsync(
        UserDeleteCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.DeleteUser);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(user.Actor);

        if (actor.Guid == user.ActorGuid && actor.ActorType == ActorType.User)
        {
            throw new FargoApplicationException($"The user '{user.ActorGuid}' cannot delete their own user.");
        }

        user.Actor.ValidateCanBeDeleted();

        userRepository.Remove(user);

        var audit = AuditLog.CreateAuditLog(actor, user.Actor, AuditLogType.EntityDeleted);

        auditLogRepository.Add(audit);
    }
}
