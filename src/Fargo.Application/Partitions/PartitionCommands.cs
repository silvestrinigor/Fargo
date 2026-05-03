using Fargo.Application.Authentication;
using Fargo.Domain;
using Fargo.Domain.Partitions;

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
    IUnitOfWork unitOfWork
) : ICommandHandler<PartitionCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        PartitionCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

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
    IUnitOfWork unitOfWork
) : ICommandHandler<PartitionDeleteCommand>
{
    public async Task Handle(
        PartitionDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeletePartition);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        partitionService.DeletePartition(partition);

        await unitOfWork.SaveChanges(cancellationToken);
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
    IUnitOfWork unitOfWork
) : ICommandHandler<PartitionUpdateCommand>
{
    public async Task Handle(
        PartitionUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

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
    }
}

#endregion Update
