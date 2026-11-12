using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos.Partitions;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Partitions;

namespace Fargo.Application.Commands.Partitions;

public sealed record PartitionUpdateCommand(
    Guid PartitionGuid,
    PartitionUpdateDto Update
) : ICommand;

public sealed class PartitionUpdateCommandHandler(
    ActorResolver actorService,
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
    }
}