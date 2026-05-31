using Fargo.Application.Identity;
using Fargo.Application.Shared.Partitions;
using Fargo.Core.Events;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

#region Create

/// <summary>
/// Command used to create a new partition from an API creation payload.
/// </summary>
public sealed record PartitionCreateCommand(
    PartitionCreateDto Create
) : ICommand<Guid>;

/// <summary>
/// Handles partition creation, including optional create-time state.
/// </summary>
public sealed class PartitionCreateCommandHandler(
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<PartitionCreateCommandHandler> logger
) : ICommandHandler<PartitionCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        PartitionCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();
        var create = command.Create;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Partition create flow started by actor {ActorGuid}.", actor.Guid);
        }

        var partition = Partition.CreatePartition(create.Name, actor);

        partitionRepository.Add(partition);

        entityEventRepository.Add(EntityEvent.EntityCreated(partition, actor.Guid));

        if (create.Description is { } description)
        {
            partition.ChangeDescription(description, actor);
        }

        var parentPartitionGuid = create.ParentPartitionGuid ?? PartitionService.GlobalPartitionGuid;

        if (partition.ParentPartitionGuid != parentPartitionGuid)
        {
            var parentPartition = await partitionRepository.GetFoundByGuid(parentPartitionGuid, cancellationToken);

            await partitionService.SetParentPartition(
                parentPartition,
                partition,
                actor,
                cancellationToken);
        }

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition create mutation completed for partition {PartitionGuid} by actor {ActorGuid}.",
                partition.Guid,
                actor.Guid);
        }

        return partition.Guid;
    }
}

#endregion Create

#region Update

/// <summary>
/// Command used to update an existing partition from an API update payload.
/// </summary>
public sealed record PartitionUpdateCommand(
    Guid PartitionGuid,
    PartitionUpdateDto Update
) : ICommand;

/// <summary>
/// Handles partition updates.
/// </summary>
public sealed class PartitionUpdateCommandHandler(
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<PartitionUpdateCommandHandler> logger
) : ICommandHandler<PartitionUpdateCommand>
{
    public async Task Handle(
        PartitionUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();
        var update = command.Update;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition update flow started for partition {PartitionGuid} by actor {ActorGuid}.",
                command.PartitionGuid,
                actor.Guid);
        }

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        partition.ValidateCanEdit(actor);

        if (update.Name is { } name)
        {
            partition.Rename(name, actor);
        }

        if (update.Description is { } description)
        {
            partition.ChangeDescription(description, actor);
        }

        if (update.ParentPartitionGuid is { } parentPartitionGuid &&
            partition.ParentPartitionGuid != parentPartitionGuid)
        {
            var parentPartition = await partitionRepository.GetFoundByGuid(parentPartitionGuid, cancellationToken);

            await partitionService.SetParentPartition(
                parentPartition,
                partition,
                actor,
                cancellationToken);
        }

        if (update.IsActive is { } isActive)
        {
            if (isActive && !partition.IsActive)
            {
                partition.Activate(actor);
                entityEventRepository.Add(EntityEvent.Activated<Partition>(partition, actor.Guid));
            }
            else if (!isActive && partition.IsActive)
            {
                partition.Deactivate(actor);
                entityEventRepository.Add(EntityEvent.Deactivated<Partition>(partition, actor.Guid));
            }
        }

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition update mutation completed for partition {PartitionGuid} by actor {ActorGuid}.",
                partition.Guid,
                actor.Guid);
        }
    }
}

#endregion Update

#region Delete

/// <summary>
/// Command used to delete a partition.
/// </summary>
public sealed record PartitionDeleteCommand(
    Guid PartitionGuid
) : ICommand;

/// <summary>
/// Handles partition deletion.
/// </summary>
public sealed class PartitionDeleteCommandHandler(
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<PartitionDeleteCommandHandler> logger
) : ICommandHandler<PartitionDeleteCommand>
{
    public async Task Handle(
        PartitionDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition delete flow started for partition {PartitionGuid} by actor {ActorGuid}.",
                command.PartitionGuid,
                actor.Guid);
        }

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        partitionService.DeletePartition(partition, actor);

        entityEventRepository.Add(EntityEvent.EntityDeleted(partition, actor.Guid));

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition delete mutation completed for partition {PartitionGuid} by actor {ActorGuid}.",
                partition.Guid,
                actor.Guid);
        }
    }
}

#endregion Delete
