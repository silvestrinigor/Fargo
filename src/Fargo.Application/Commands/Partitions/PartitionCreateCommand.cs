using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos.Partitions;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;

namespace Fargo.Application.Commands.Partitions;

public sealed record PartitionCreateCommand(PartitionCreateDto Create) : ICommand<Guid>;

public sealed class PartitionCreateCommandHandler(
    ActorResolver actorService,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<PartitionCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        PartitionCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.CreatePartition);

        // When the caller does not specify a parent, default to the global
        // partition. The actor must have access to the global partition to use
        // this default; otherwise they must name a parent they can access.
        var parentPartitionGuid =
            command.Create.ParentPartitionGuid ?? FargoCoreWellKnowGuids.GlobalPartitionGuid;

        var parentPartition = await partitionRepository.GetByGuidAsync(parentPartitionGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(parentPartition, parentPartitionGuid);

        if (command.Create.ParentPartitionGuid is null && !actor.HasAccess(parentPartition))
        {
            throw new FargoApplicationException(
                "No parent partition was provided and the default global partition is not "
                + "accessible to you. Specify a parent partition you have access to.",
                FargoErrorType.AccessDenied);
        }

        actor.ThrowIfAccessDenied(parentPartition);

        var newPartition = Partition.CreatePartition(command.Create.Name, parentPartition);

        newPartition.Description = command.Create.Description ?? Description.Empty;

        partitionRepository.Add(newPartition);

        return newPartition.Guid;
    }
}
