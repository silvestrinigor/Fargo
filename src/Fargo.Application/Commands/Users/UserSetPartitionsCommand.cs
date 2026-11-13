using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos;
using Fargo.Core.Entities;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Actors.Users;
using Fargo.Core.Entities.Partitions;

namespace Fargo.Application.Commands.Users;

public sealed record UserSetPartitionsCommand(Guid UserGuid, PartitionsChangeDto Change) : ICommand;

public sealed class UserSetPartitionsCommandHandler(
    ActorResolver actorService,
    IUserRepository userRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<UserSetPartitionsCommand>
{
    public async Task HandleAsync(
        UserSetPartitionsCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangeUserPartitions);

        var plan = PartitionsChangePlan.From(command.Change);

        var user = await userRepository.GetByGuidAsync(command.UserGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(user, command.UserGuid, EntityType.Actor);

        actor.ThrowIfAccessDenied(user.Actor);

        await plan.ApplyAsync(
            actor,
            partitionRepository,
            user.Actor.AddPartition,
            user.Actor.RemovePartition,
            user.Actor.AddPartitionAccess,
            user.Actor.RemovePartitionAccess,
            cancellationToken);
    }
}
