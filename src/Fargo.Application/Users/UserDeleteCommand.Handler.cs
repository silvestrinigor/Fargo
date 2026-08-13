using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Actors;
using Fargo.Core.Shared.Entities;
using Fargo.Core.Users;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Users;

public sealed class UserDeleteCommandHandler(
    ActorResolver actorService,
    IUserRepository userRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<UserDeleteCommandHandler> logger
) : ICommandHandler<UserDeleteCommand>
{
    public async Task HandleAsync(
        UserDeleteCommand command, CancellationToken cancellationToken = default)
    {
        logger.UserDeleteCompleted(command.UserGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.DeleteUser);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.User);

        actor.ThrowIfAccessDenied(user);

        if (actor.Guid == user.Guid && actor.ActorType == ActorType.User)
        {
            throw new FargoApplicationException($"The user '{user.Guid}' cannot delete their own user.");
        }

        UserService.ValidateUserCanBeDeleted(user);

        userRepository.Remove(user);

        var audit = AuditLog.CreateAuditLog(actor, user, ActionType.DeleteUser);

        auditLogRepository.Add(audit);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UserDeleteCompleted(command.UserGuid, currentActor.Guid);
    }
}
