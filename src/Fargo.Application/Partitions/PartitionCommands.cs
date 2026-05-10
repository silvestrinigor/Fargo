using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

#region Create

public sealed record PartitionCreateCommand(
    PartitionCreateDto Partition
) : ICommand<Guid>;

public sealed class PartitionCreateCommandHandler(
    ActorService actorService,
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    ILogger<PartitionCreateCommandHandler> logger
) : ICommandHandler<PartitionCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        PartitionCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Partition create flow started by actor {ActorGuid}.", actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreatePartition);

        var partition = new Partition
        {
            Name = command.Partition.Name,
            Description = command.Partition.Description ?? Description.Empty
        };

        var parentPartitionGuid = command.Partition.ParentPartitionGuid ?? PartitionService.GlobalPartitionGuid;

        var parentPartition = await partitionRepository.GetFoundByGuid(parentPartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(parentPartition.Guid);

        await partitionService.SetParentPartition(parentPartition, partition, cancellationToken);

        partitionRepository.Add(partition);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition create flow completed for partition {PartitionGuid} by actor {ActorGuid}. ParentPartitionGuid: {ParentPartitionGuid}.",
                partition.Guid,
                actor.Guid,
                parentPartition.Guid);
        }

        return partition.Guid;
    }
}

#endregion Create

#region Delete

public sealed record PartitionDeleteCommand(
    Guid PartitionGuid
) : ICommand;

public sealed class PartitionDeleteCommandHandler(
    ActorService actorService,
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    ILogger<PartitionDeleteCommandHandler> logger
) : ICommandHandler<PartitionDeleteCommand>
{
    public async Task Handle(
        PartitionDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition delete flow started for partition {PartitionGuid} by actor {ActorGuid}.",
                command.PartitionGuid,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeletePartition);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        partitionService.DeletePartition(partition);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition delete flow completed for partition {PartitionGuid} by actor {ActorGuid}.",
                partition.Guid,
                actor.Guid);
        }
    }
}

#endregion Delete

#region Update

public sealed record PartitionUpdateCommand(
    Guid PartitionGuid,
    PartitionUpdateDto Partition
) : ICommand;

public sealed class PartitionUpdateCommandHandler(
    ActorService actorService,
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    ILogger<PartitionUpdateCommandHandler> logger
) : ICommandHandler<PartitionUpdateCommand>
{
    public async Task Handle(
        PartitionUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition update flow started for partition {PartitionGuid} by actor {ActorGuid}.",
                command.PartitionGuid,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditPartition);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        if (command.Partition.ParentPartitionGuid is not null &&
            partition.ParentPartitionGuid != command.Partition.ParentPartitionGuid)
        {
            var parentPartition = await partitionRepository.GetFoundByGuid(
                command.Partition.ParentPartitionGuid.Value,
                cancellationToken
            );

            actor.ValidateHasPartitionAccess(parentPartition.Guid);

            await partitionService.SetParentPartition(parentPartition, partition, cancellationToken);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Partition update flow moved partition {PartitionGuid} under parent partition {ParentPartitionGuid}.",
                    partition.Guid,
                    parentPartition.Guid);
            }
        }

        if (command.Partition.Name is not null && partition.Name != command.Partition.Name.Value)
        {
            partition.Name = command.Partition.Name.Value;
        }

        if (command.Partition.Description is not null && partition.Description != command.Partition.Description.Value)
        {
            partition.Description = command.Partition.Description.Value;
        }

        if (command.Partition.IsActive is not null && partition.IsActive != command.Partition.IsActive.Value)
        {
            partition.IsActive = command.Partition.IsActive.Value;
        }

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition update flow completed for partition {PartitionGuid} by actor {ActorGuid}. IsActive: {IsActive}.",
                partition.Guid,
                actor.Guid,
                partition.IsActive);
        }
    }
}

#endregion Update
