using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;

namespace Fargo.Application.Commands.Partitions;

/// <remarks>
/// Implements <see cref="ITransactionalCommand"/>: the handler takes the partition hierarchy
/// advisory lock, which must be held until commit. A partition always has a parent, so there is no
/// command that clears it. Setting the parent a partition already has does nothing.
/// </remarks>
public sealed record PartitionSetParentCommand(Guid PartitionGuid, Guid ParentPartitionGuid) : ITransactionalCommand;

public sealed class PartitionSetParentCommandHandler(
    ActorResolver actorService,
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<PartitionSetParentCommand>
{
    // The command is transactional, so the registration layer wraps this handler in the
    // transactional unit-of-work decorator: the whole body runs in one transaction that the
    // decorator commits once on success. The handler must therefore NOT open its own; it only takes
    // the hierarchy advisory lock, which the ambient transaction keeps until commit.
    public async Task HandleAsync(PartitionSetParentCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.ChangePartitionParent);

        var partition = await partitionRepository.GetByGuidAsync(command.PartitionGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(partition, command.PartitionGuid);

        actor.ThrowIfAccessDenied(partition);

        if (partition.ParentPartitionGuid == command.ParentPartitionGuid)
        {
            return;
        }

        var parentPartition = await partitionRepository.GetByGuidAsync(command.ParentPartitionGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(parentPartition, command.ParentPartitionGuid);

        actor.ThrowIfAccessDenied(parentPartition);

        // The global lock makes the read-check-write (no cycle, then set the parent) atomic: it is
        // held until the decorator's transaction commits, so concurrent re-parents cannot each pass
        // the check and together form a cycle.
        await partitionRepository.AcquireHierarchyLockAsync(cancellationToken);

        await partitionService.ValidateParentPartitionHierarchyAssignmentAsync(parentPartition, partition, cancellationToken);

        partition.SetParentPartition(parentPartition);
    }
}
