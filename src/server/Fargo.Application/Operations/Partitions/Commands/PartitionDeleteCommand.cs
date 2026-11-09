using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;


namespace Fargo.Application.Operations.Partitions;

/// <summary>
/// Command used to delete a partition.
/// </summary>
public sealed record PartitionDeleteCommand(Guid PartitionGuid) : ICommand;

public sealed class PartitionDeleteCommandHandler(
    PartitionService partitionService,
    ActorResolver actorService,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<PartitionDeleteCommand>
{
    public async Task HandleAsync(
        PartitionDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.DeletePartition);

        var partitionToDelete = await partitionRepository.GetByGuidAsync(command.PartitionGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(partitionToDelete, command.PartitionGuid);

        actor.ThrowIfAccessDenied(partitionToDelete);

        if (partitionToDelete.ParentPartitionGuid is null)
        {
            throw new FargoApplicationException("Cannot delete a partition with no parent partition.");
        }

        var parentPartition = await partitionRepository.GetByGuidAsync(partitionToDelete.ParentPartitionGuid.Value, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(parentPartition, partitionToDelete.ParentPartitionGuid.Value);

        actor.ThrowIfAccessDenied(parentPartition);

        await partitionService.ValidatePartitionCanBeDeletedAsync(partitionToDelete, cancellationToken);

        partitionRepository.Remove(partitionToDelete);
    }
}
