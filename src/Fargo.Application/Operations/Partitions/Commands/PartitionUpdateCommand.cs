using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Operations.Partitions.Dtos;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;


namespace Fargo.Application.Operations.Partitions;

/// <summary>
/// Command used to update an existing partition from an API update payload.
/// </summary>
/// <remarks>
/// Implements <see cref="ITransactionalCommand"/>: on the re-parent path the
/// handler acquires a hierarchy advisory lock that must be held until commit, so
/// it runs inside a single explicit transaction owned by the pipeline.
/// </remarks>
public sealed record PartitionUpdateCommand(
    Guid PartitionGuid,
    PartitionUpdateDto Update
) : ITransactionalCommand;

public sealed class PartitionUpdateCommandHandler(
    ActorResolver actorService,
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
) : ICommandHandler<PartitionUpdateCommand>
{
    // This handler's command implements ITransactionalCommand, so the registration
    // layer wraps it in the transactional unit-of-work decorator: its whole body runs
    // inside one explicit transaction that the decorator commits once on success. It
    // must therefore NOT open its own transaction; it simply acquires the hierarchy
    // advisory lock on the re-parent path, and the ambient transaction keeps that lock
    // held until the decorator commits.
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

            // Acquire the global partition-hierarchy lock before the cycle check so
            // the read-check-write (validate no cycle -> set parent) is atomic: the
            // lock is held until the decorator's transaction commits, so concurrent
            // re-parents cannot each pass the check and together form a cycle.
            await partitionRepository.AcquireHierarchyLockAsync(cancellationToken);

            await partitionService.ValidateParentPartitionHierarchyAssignmentAsync(parentPartitionToSet, partitionToEdit, cancellationToken);

            partitionToEdit.SetParentPartition(parentPartitionToSet);
        }
    }
}
