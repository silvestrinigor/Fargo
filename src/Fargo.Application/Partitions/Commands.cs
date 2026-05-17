using Fargo.Application.Identity;
using Fargo.Core;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Partitions;

#region Create

/// <summary>
/// Command used to create a new partition.
/// </summary>
/// <param name="Name">
/// Partition name.
/// </param>
public sealed record PartitionCreateCommand(
    Name Name
) : ICommand<Guid>;

/// <summary>
/// Handles partition creation.
/// </summary>
/// <remarks>
/// Validates permissions and stores the new partition.
/// </remarks>
public sealed class PartitionCreateCommandHandler(
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<PartitionCreateCommandHandler> logger
) : ICommandHandler<PartitionCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        PartitionCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Partition create flow started by actor {ActorGuid}.", actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.CreatePartition);

        var partition = new Partition(command.Name);

        partitionRepository.Add(partition);

        entityEventRepository.Add(EntityEvent.EntityCreated<Partition>(partition, actor.ActorGuid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition create mutation completed for partition {PartitionGuid} by actor {ActorGuid}.",
                partition.Guid,
                actor.ActorGuid);
        }

        return partition.Guid;
    }
}

#endregion Create

#region Delete

/// <summary>
/// Command used to delete a partition.
/// </summary>
/// <param name="PartitionGuid">
/// Partition unique identifier.
/// </param>
public sealed record PartitionDeleteCommand(
    Guid PartitionGuid
) : ICommand;

/// <summary>
/// Handles partition deletion.
/// </summary>
/// <remarks>
/// Validates permissions and delegates deletion rules
/// to the partition domain service.
/// </remarks>
public sealed class PartitionDeleteCommandHandler(
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<PartitionDeleteCommandHandler> logger
) : ICommandHandler<PartitionDeleteCommand>
{
    public async Task Handle(
        PartitionDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition delete flow started for partition {PartitionGuid} by actor {ActorGuid}.",
                command.PartitionGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.DeletePartition);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        partitionService.DeletePartition(partition);

        entityEventRepository.Add(EntityEvent.EntityDeleted<Partition>(partition, actor.ActorGuid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition delete mutation completed for partition {PartitionGuid} by actor {ActorGuid}.",
                partition.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Delete

#region Name

/// <summary>
/// Command used to rename a partition.
/// </summary>
/// <param name="PartitionGuid">
/// Partition unique identifier.
/// </param>
/// <param name="Name">
/// New partition name.
/// </param>
public sealed record PartitionRenameCommand(
    Guid PartitionGuid,
    Name Name
) : ICommand;

/// <summary>
/// Handles partition rename.
/// </summary>
/// <remarks>
/// Validates permissions and updates the partition name.
/// </remarks>
public sealed class PartitionRenameCommandHandler(
    IPartitionRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<PartitionRenameCommandHandler> logger
) : ICommandHandler<PartitionRenameCommand>
{
    public async Task Handle(
        PartitionRenameCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition rename flow started for partition {PartitionGuid} by actor {ActorGuid}.",
                command.PartitionGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditPartition);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        if (partition.Name == command.Name)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Partition rename flow skipped for partition {PartitionGuid}; name is already requested value.",
                    partition.Guid);
            }

            return;
        }

        partition.Rename(command.Name);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition rename mutation completed for partition {PartitionGuid} by actor {ActorGuid}.",
                partition.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Name

#region Description

/// <summary>
/// Command used to change the partition description.
/// </summary>
/// <param name="PartitionGuid">
/// Partition unique identifier.
/// </param>
/// <param name="Description">
/// New partition description.
/// </param>
public sealed record PartitionChangeDescriptionCommand(
    Guid PartitionGuid,
    Description Description
) : ICommand;

/// <summary>
/// Handles partition description changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates the description.
/// </remarks>
public sealed class PartitionChangeDescriptionCommandHandler(
    IPartitionRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<PartitionChangeDescriptionCommandHandler> logger
) : ICommandHandler<PartitionChangeDescriptionCommand>
{
    public async Task Handle(
        PartitionChangeDescriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition description change flow started for partition {PartitionGuid} by actor {ActorGuid}.",
                command.PartitionGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditPartition);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        if (partition.Description == command.Description)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Partition description change flow skipped for partition {PartitionGuid}; description is already requested value.",
                    partition.Guid);
            }

            return;
        }

        partition.ChangeDescription(command.Description);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition description change mutation completed for partition {PartitionGuid} by actor {ActorGuid}.",
                partition.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Description

#region Parent

/// <summary>
/// Command used to change the partition parent.
/// </summary>
/// <param name="PartitionGuid">
/// Partition unique identifier.
/// </param>
/// <param name="ParentPartitionGuid">
/// New parent partition unique identifier.
/// </param>
public sealed record PartitionSetParentCommand(
    Guid PartitionGuid,
    Guid ParentPartitionGuid
) : ICommand;

/// <summary>
/// Handles partition parent changes.
/// </summary>
/// <remarks>
/// Validates permissions and delegates hierarchy rules
/// to the partition domain service.
/// </remarks>
public sealed class PartitionSetParentCommandHandler(
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<PartitionSetParentCommandHandler> logger
) : ICommandHandler<PartitionSetParentCommand>
{
    public async Task Handle(
        PartitionSetParentCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition parent mutation started for partition {PartitionGuid} by actor {ActorGuid}.",
                command.PartitionGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditPartition);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        if (partition.ParentPartitionGuid == command.ParentPartitionGuid)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Partition parent mutation skipped for partition {PartitionGuid}; parent partition is already {ParentPartitionGuid}.",
                    partition.Guid,
                    partition.ParentPartitionGuid);
            }

            return;
        }

        var parentPartition = await partitionRepository.GetFoundByGuid(command.ParentPartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(parentPartition.Guid);

        await partitionService.SetParentPartition(parentPartition, partition, cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition parent mutation completed for partition {PartitionGuid} by actor {ActorGuid}. ParentPartitionGuid: {ParentPartitionGuid}.",
                partition.Guid,
                actor.ActorGuid,
                partition.ParentPartitionGuid);
        }
    }
}

#endregion Parent

#region Activate

/// <summary>
/// Command used to activate a partition.
/// </summary>
/// <param name="PartitionGuid">
/// Partition unique identifier.
/// </param>
public sealed record PartitionActivateCommand(
    Guid PartitionGuid
) : ICommand;

/// <summary>
/// Handles partition activation.
/// </summary>
/// <remarks>
/// Validates permissions and activates the partition.
/// </remarks>
public sealed class PartitionActivateCommandHandler(
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<PartitionActivateCommandHandler> logger
) : ICommandHandler<PartitionActivateCommand>
{
    public async Task Handle(
        PartitionActivateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition activation flow started for partition {PartitionGuid} by actor {ActorGuid}.",
                command.PartitionGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditPartition);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        if (partition.IsActive)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Partition activation flow skipped for partition {PartitionGuid}; partition is already active.",
                    partition.Guid);
            }

            return;
        }

        partition.Activate();

        entityEventRepository.Add(EntityEvent.Activated<Partition>(partition, actor.ActorGuid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition activation mutation completed for partition {PartitionGuid} by actor {ActorGuid}.",
                partition.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Activate

#region Deactivate

/// <summary>
/// Command used to deactivate a partition.
/// </summary>
/// <param name="PartitionGuid">
/// Partition unique identifier.
/// </param>
public sealed record PartitionDeactivateCommand(
    Guid PartitionGuid
) : ICommand;

/// <summary>
/// Handles partition deactivation.
/// </summary>
/// <remarks>
/// Validates permissions and deactivates the partition.
/// </remarks>
public sealed class PartitionDeactivateCommandHandler(
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<PartitionDeactivateCommandHandler> logger
) : ICommandHandler<PartitionDeactivateCommand>
{
    public async Task Handle(
        PartitionDeactivateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition deactivation flow started for partition {PartitionGuid} by actor {ActorGuid}.",
                command.PartitionGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditPartition);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        if (!partition.IsActive)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Partition deactivation flow skipped for partition {PartitionGuid}; partition is already inactive.",
                    partition.Guid);
            }

            return;
        }

        partition.Deactivate();

        entityEventRepository.Add(EntityEvent.Deactivated<Partition>(partition, actor.ActorGuid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition deactivation mutation completed for partition {PartitionGuid} by actor {ActorGuid}.",
                partition.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Deactivate
