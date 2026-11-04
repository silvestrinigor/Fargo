using Fargo.Application.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;

namespace Fargo.Application.Partitions;

public sealed class PartitionUpdateCommandHandler(
    ActorResolver actorService,
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<PartitionUpdateCommand>
{
    public async Task HandleAsync(PartitionUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditPartition);

        var partitionToEdit = await partitionRepository.GetByGuidAsync(command.PartitionGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(partitionToEdit, command.PartitionGuid);

        actor.ThrowIfAccessDenied(partitionToEdit);

        if (command.Update.Name is { } name)
        {
            partitionToEdit.Name = name;
        }

        if (command.Update.Description is { } description)
        {
            partitionToEdit.Description = description;
        }

        if (command.Update.ParentPartitionGuid is { } parentPartitionGuidToSet)
        {
            var parentPartitionToSet = await partitionRepository.GetByGuidAsync(parentPartitionGuidToSet, cancellationToken);

            EntityNotFoundFargoApplicationException.ThrowIfNull(parentPartitionToSet, parentPartitionGuidToSet);

            actor.ThrowIfAccessDenied(parentPartitionToSet);

            await partitionService.ValidateParentPartitionHierarchyAssignmentAsync(parentPartitionToSet, partitionToEdit, cancellationToken);

            partitionToEdit.SetParentPartition(parentPartitionToSet);
        }
    }
}
