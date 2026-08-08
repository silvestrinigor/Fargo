using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Partitions;
using Fargo.Core.Shared.Actions;
using Fargo.Core.Shared.Entities;
using Fargo.Core.Shared.Informations;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionCreateCommandHandler(
    ActorService actorService,
    IPartitionRepository partitionRepository,
    IUnitOfWork unitOfWork, ICurrentActor currentActor,
    ILogger<PartitionCreateCommandHandler> logger
) : ICommandHandler<PartitionCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        PartitionCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.CreateStarted(currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.CreatePartition);

        var parentPartition = await partitionRepository.GetByGuidAsync(command.Create.ParentPartitionGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(parentPartition, command.Create.ParentPartitionGuid, EntityType.Partition);

        actor.ThrowIfAccessDenied(parentPartition);

        var newPartition = Partition.CreatePartition(command.Create.Name, parentPartition);

        newPartition.Description = command.Create.Description ?? Description.Empty;

        partitionRepository.Add(newPartition);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CreateCompleted(newPartition.Guid, currentActor.Guid);

        return newPartition.Guid;
    }
}
