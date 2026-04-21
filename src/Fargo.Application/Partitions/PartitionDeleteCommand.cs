using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Partitions;

namespace Fargo.Application.Partitions;

/// <summary>
/// Command used to delete an existing partition.
/// </summary>
/// <param name="PartitionGuid">
/// The unique identifier of the partition to be deleted.
/// </param>
public sealed record PartitionDeleteCommand(
        Guid PartitionGuid
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="PartitionDeleteCommand"/>.
/// </summary>
/// <remarks>
/// This handler deletes an existing partition after validating that the
/// current actor is active, has permission to delete partitions,
/// and has access to the specified partition.
/// </remarks>
public sealed class PartitionDeleteCommandHandler(
        PartitionService partitionService,
        ActorService actorService,
        IPartitionRepository partitionRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IFargoEventPublisher eventPublisher
        ) : ICommandHandler<PartitionDeleteCommand>
{
    /// <summary>
    /// Executes the command to delete a partition.
    /// </summary>
    /// <param name="command">
    /// The command containing the identifier of the partition to delete.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="command"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current actor cannot be resolved or is not active.
    /// </exception>
    /// <exception cref="UserNotAuthorizedFargoDomainException">
    /// Thrown when the actor does not have permission to delete partitions.
    /// </exception>
    /// <exception cref="PartitionNotFoundFargoApplicationException">
    /// Thrown when the specified partition does not exist or the actor
    /// does not have access to it.
    /// </exception>
    public async Task Handle(PartitionDeleteCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeletePartition);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        partitionService.DeletePartition(partition);

        await unitOfWork.SaveChanges(cancellationToken);

        await eventPublisher.PublishPartitionDeleted(partition.Guid, cancellationToken);
    }
}
