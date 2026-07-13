using Fargo.Application.Identity;
using Fargo.Core;
using Fargo.Core.Actors;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionCreateCommandHandler(
    ActorService actorService,
    PartitionService partitionService,
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

        ActorNotFoundFargoException.ThrowIfNull(actor, currentActor.ActorId);

        actor.ThrowIfPermissionDenied(ActionType.CreatePartition);

        var create = command.Create;

        var partition = Partition.CreatePartition(create.Name);

        partition.Description = create.Description ?? Description.Empty;

        var parentPartitionGuid = create.ParentPartitionGuid ?? FargoConstantGuids.GlobalPartitionGuid;

        if (partition.ParentPartitionGuid != parentPartitionGuid)
        {
            var parentPartition = await partitionRepository.GetByGuidAsync(parentPartitionGuid, cancellationToken);

            EntityNotFoundFargoException.ThrowIfNull(parentPartition, parentPartitionGuid, EntityType.Partition);

            await partitionService.SetParentPartition(
                parentPartition, partition, cancellationToken);
        }

        partitionRepository.Add(partition);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CreateCompleted(partition.Guid, currentActor.ActorId);

        return partition.Guid;
    }
}
