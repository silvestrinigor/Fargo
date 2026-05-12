using Fargo.Application.Identity;
using Fargo.Core;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Fargo.Application.Partitions;

#region DTOs

public sealed record PartitionDto(
    Guid Guid,
    Name Name,
    Description Description,
    Guid? ParentPartitionGuid,
    bool IsActive,
    Guid? EditedByGuid
);

public sealed record PartitionCreateDto(
    Name Name,
    Description? Description = null,
    Guid? ParentPartitionGuid = null
);

public sealed record PartitionUpdateDto(
    Name? Name = null,
    Description? Description = null,
    Guid? ParentPartitionGuid = null,
    bool? IsActive = null
);

public static class PartitionDtoMappings
{
    public static readonly Expression<Func<Partition, PartitionDto>> Projection = partition => new PartitionDto(
        partition.Guid,
        partition.Name,
        partition.Description,
        partition.ParentPartitionGuid,
        partition.IsActive,
        partition.EditedByGuid);
}

#endregion DTOs

#region Exceptions

public class PartitionNotFoundFargoApplicationException(Guid partitionGuid)
    : FargoApplicationException($"Partition with guid '{partitionGuid}' was not found.")
{
    public Guid PartitionGuid { get; } = partitionGuid;
}

public sealed class PartitionAccessDeniedFargoApplicationException(
    Guid partitionGuid,
    Guid userGuid
) : FargoApplicationException(
    $"User '{userGuid}' does not have access to partition '{partitionGuid}'.")
{
    public Guid PartitionGuid { get; } = partitionGuid;

    public Guid UserGuid { get; } = userGuid;
}

public sealed class PartitionedEntityAccessDeniedFargoApplicationException(
    Guid entityGuid,
    Guid userGuid
) : FargoApplicationException(
    $"User '{userGuid}' does not have access to entity '{entityGuid}'.")
{
    public Guid EntityGuid { get; } = entityGuid;

    public Guid UserGuid { get; } = userGuid;
}

#endregion Exceptions

#region Repositories

public interface IPartitionQueryRepository
{
    Task<PartitionDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<PartitionDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );
}
public static class PartitionRepositoryExtensions
{
    extension(IPartitionRepository repository)
    {
        public async Task<Partition> GetFoundByGuid(
            Guid partitionGuid,
            CancellationToken cancellationToken = default
        )
        {
            var partition = await repository.GetByGuid(partitionGuid, cancellationToken)
                ?? throw new PartitionNotFoundFargoApplicationException(partitionGuid);

            return partition;
        }
    }
}

#endregion Repositories

#region Create Delete Update

#region Create

public sealed record PartitionCreateCommand(
    PartitionCreateDto Partition
) : ICommand<Guid>;

public sealed class PartitionCreateCommandHandler(
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
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
                actor.ActorGuid,
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
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
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

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition delete flow completed for partition {PartitionGuid} by actor {ActorGuid}.",
                partition.Guid,
                actor.ActorGuid);
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
    PartitionService partitionService,
    IPartitionRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<PartitionUpdateCommandHandler> logger
) : ICommandHandler<PartitionUpdateCommand>
{
    public async Task Handle(
        PartitionUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Partition update flow started for partition {PartitionGuid} by actor {ActorGuid}.",
                command.PartitionGuid,
                actor.ActorGuid);
        }

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
                actor.ActorGuid,
                partition.IsActive);
        }
    }
}

#endregion Update

#endregion Create Delete Update

#region Queries

#region Single

public sealed record PartitionSingleQuery(
    Guid PartitionGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<PartitionDto?>;

public sealed class PartitionSingleQueryHandler(
    IPartitionQueryRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<PartitionSingleQueryHandler> logger
) : IQueryHandler<PartitionSingleQuery, PartitionDto?>
{
    public async Task<PartitionDto?> Handle(
        PartitionSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Partition single query started for partition {PartitionGuid} by actor {ActorGuid}.",
                query.PartitionGuid,
                actor.ActorGuid);
        }

        var partition = await partitionRepository.GetInfoByGuid(
            query.PartitionGuid,
            query.AsOfDateTime,
            childOfAnyOfThesePartitions: null,
            notChildOfAnyPartition: null,
            cancellationToken
        );

        if (partition is not null)
        {
            actor.ValidateHasPartitionAccess(partition.Guid);
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Partition single query completed for partition {PartitionGuid} by actor {ActorGuid}. Found: {Found}.",
                query.PartitionGuid,
                actor.ActorGuid,
                partition is not null);
        }

        return partition;
    }
}

#endregion Single

#region Many

public sealed record PartitionsQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    bool? NotChildOfAnyPartition = null
) : IQuery<IReadOnlyCollection<PartitionDto>>;

public sealed class PartitionsQueryHandler(
    IPartitionQueryRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<PartitionsQueryHandler> logger
) : IQueryHandler<PartitionsQuery, IReadOnlyCollection<PartitionDto>>
{
    public async Task<IReadOnlyCollection<PartitionDto>> Handle(
        PartitionsQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);
        var pagination = query.WithPagination;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Partitions query started for actor {ActorGuid}. Page: {Page}. Limit: {Limit}.",
                actor.ActorGuid,
                pagination.Page,
                pagination.Limit);
        }

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccesses,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var partitions = await partitionRepository.GetManyInfo(
            pagination,
            query.TemporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Partitions query completed for actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}. EffectivePartitionCount: {EffectivePartitionCount}. ResultCount: {ResultCount}.",
                actor.ActorGuid,
                query.ChildOfAnyOfThesePartitions?.Count ?? 0,
                childOfAnyOfThesePartitions?.Count ?? 0,
                partitions.Count);
        }

        return partitions;
    }
}

#endregion Many

#endregion Queries
