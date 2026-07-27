using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionCreateCommandHandler(
    ActorService actorService,
    IPartitionRepository partitionRepository,
    IUnitOfWork unitOfWork,
    ICurrentActor currentActor,
    ILogger<PartitionCreateCommandHandler> logger
) : ICommandHandler<PartitionCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        PartitionCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.CreateStarted(currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.ActorId);

        actor.ThrowIfPermissionDenied(ActionType.CreatePartition);

        var parentPartition = await partitionRepository.GetByGuidAsync(command.Create.ParentPartitionGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(parentPartition, command.Create.ParentPartitionGuid, EntityType.Partition);

        actor.ThrowIfAccessDeniedToPartition(parentPartition);

        var newPartition = Partition.CreatePartition(command.Create.Name, parentPartition);

        newPartition.Description = command.Create.Description ?? Description.Empty;

        partitionRepository.Add(newPartition);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CreateCompleted(newPartition.Guid, currentActor.ActorId);

        return newPartition.Guid;
    }
}
