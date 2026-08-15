using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

public sealed class PartitionCreateCommandHandler(
    ActorResolver actorService,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
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

        var partitionAudit = AuditLog.CreateAuditLog(actor, newPartition, ActionType.CreatePartition);

        partitionAudit.Metadata.AddName(newPartition.Name);

        newPartition.Description = command.Create.Description ?? Description.Empty;

        partitionAudit.Metadata.AddDescription(newPartition.Description);

        auditLogRepository.Add(partitionAudit);

        partitionRepository.Add(newPartition);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CreateCompleted(newPartition.Guid, currentActor.Guid);

        return newPartition.Guid;
    }
}
