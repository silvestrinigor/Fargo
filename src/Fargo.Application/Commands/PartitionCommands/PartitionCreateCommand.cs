using Fargo.Application.Extensions;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Enums;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Commands.PartitionCommands;

/// <summary>
/// Command used to create a new <see cref="Partition"/>.
/// </summary>
/// <param name="Name">
/// The name of the partition to be created.
/// </param>
/// <param name="Description">
/// The optional description of the partition.
/// When not provided, <see cref="Description.Empty"/> is used.
/// </param>
/// <param name="ParentPartitionGuid">
/// The optional identifier of the parent partition.
/// When provided, the new partition should be created as a child of the specified parent partition.
/// </param>
public sealed record PartitionCreateCommand(
        Name Name,
        Description? Description = null,
        Guid? ParentPartitionGuid = null
        ) : ICommand<Guid>;

/// <summary>
/// Handles the execution of <see cref="PartitionCreateCommand"/>.
/// </summary>
/// <remarks>
/// This handler creates a new <see cref="Partition"/> after validating that the
/// current actor is active and has permission to create partitions.
/// </remarks>
public sealed class PartitionCreateCommandHandler(
        ActorService actorService,
        PartitionService partitionService,
        IUserRepository userRepository,
        IPartitionRepository partitionRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork
        ) : ICommandHandler<PartitionCreateCommand, Guid>
{
    /// <summary>
    /// Executes the command to create a new partition.
    /// </summary>
    /// <param name="command">
    /// The command containing the data required to create the partition.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The unique identifier of the created partition.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="command"/> is <see langword="null"/>.
    /// </exception>
    public async Task<Guid> Handle(PartitionCreateCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreatePartition);

        var partition = new Partition
        {
            Name = command.Name,
            Description = command.Description ?? Description.Empty
        };

        var parentPartition = await partitionRepository.GetFoundByGuid(
                command.ParentPartitionGuid ?? PartitionService.GlobalPartitionGuid,
                cancellationToken
                );

        actor.ValidateHasPartitionAccess(parentPartition.Guid);

        await partitionService.SetParentPartition(parentPartition, partition, cancellationToken);

        partitionRepository.Add(partition);

        await unitOfWork.SaveChanges(cancellationToken);

        return partition.Guid;
    }
}
