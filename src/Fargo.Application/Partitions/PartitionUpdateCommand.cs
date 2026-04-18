using Fargo.Application.Events;
using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain;
using Fargo.Domain.Partitions;

namespace Fargo.Application.Commands.PartitionCommands;

/// <summary>
/// Command used to update an existing partition.
/// </summary>
/// <param name="PartitionGuid">
/// The unique identifier of the partition to update.
/// </param>
/// <param name="Partition">
/// The data containing the fields to be updated in the partition.
/// Only the properties explicitly provided in <see cref="PartitionUpdateModel"/>
/// will be modified. Properties with <see langword="null"/> values are ignored.
/// </param>
/// <remarks>
/// This command supports partial updates of a partition, including:
/// <list type="bullet">
/// <item>
/// <description>updating the partition description</description>
/// </item>
/// <item>
/// <description>moving the partition to a different parent partition</description>
/// </item>
/// </list>
///
/// When <see cref="PartitionUpdateModel.ParentPartitionGuid"/> is provided,
/// the partition is moved from its current parent to the specified parent,
/// subject to domain validation rules.
/// </remarks>
public sealed record PartitionUpdateCommand(
        Guid PartitionGuid,
        PartitionUpdateModel Partition
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="PartitionUpdateCommand"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for orchestrating the update of a partition,
/// including permission validation, access control, and delegation of
/// domain-specific behavior to <see cref="PartitionService"/>.
///
/// <para/>
/// The update process includes:
/// <list type="number">
/// <item>
/// <description>resolving the current actor</description>
/// </item>
/// <item>
/// <description>validating permissions for partition modification</description>
/// </item>
/// <item>
/// <description>retrieving the target partition</description>
/// </item>
/// <item>
/// <description>optionally moving the partition within the hierarchy</description>
/// </item>
/// <item>
/// <description>applying partial updates to mutable properties</description>
/// </item>
/// </list>
///
/// <para/>
/// Domain invariants such as hierarchy consistency (e.g., preventing cycles)
/// and business rules related to partition movement must be enforced
/// by the domain layer.
/// </remarks>
public sealed class PartitionUpdateCommandHandler(
        PartitionService partitionService,
        ActorService actorService,
        IPartitionRepository partitionRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork,
        IFargoEventPublisher eventPublisher
        ) : ICommandHandler<PartitionUpdateCommand>
{
    /// <summary>
    /// Executes the command to update an existing partition.
    /// </summary>
    /// <param name="command">
    /// The command containing the data required to update the partition.
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
    /// Thrown when the current actor does not have permission to update partitions.
    /// </exception>
    /// <exception cref="PartitionNotFoundFargoApplicationException">
    /// Thrown when the specified partition or the target parent partition does not exist
    /// or the actor does not have access to it.
    /// </exception>
    /// <remarks>
    /// This operation updates the mutable properties of a partition.
    ///
    /// <para/>
    /// When <see cref="PartitionUpdateModel.ParentPartitionGuid"/> is provided and differs
    /// from the current parent, the partition is moved to a new parent partition.
    /// This operation enforces access validation for both the current partition and
    /// the target parent partition.
    ///
    /// <para/>
    /// Properties not provided in <see cref="PartitionUpdateModel"/> are not modified.
    /// </remarks>
    public async Task Handle(PartitionUpdateCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var actor = await actorService.GetAuthorizedActorByGuid(
                currentUser.UserGuid,
                cancellationToken);

        actor.ValidateHasPermission(ActionType.EditPartition);

        var partition = await partitionRepository.GetFoundByGuid(
                command.PartitionGuid,
                cancellationToken);

        actor.ValidateHasPartitionAccess(partition.Guid);

        if (command.Partition.ParentPartitionGuid is not null &&
                partition.ParentPartitionGuid != command.Partition.ParentPartitionGuid)
        {
            var parentPartition = await partitionRepository.GetFoundByGuid(
                    command.Partition.ParentPartitionGuid.Value,
                    cancellationToken);

            actor.ValidateHasPartitionAccess(parentPartition.Guid);

            await partitionService.SetParentPartition(
                    parentPartition,
                    partition,
                    cancellationToken);
        }

        if (command.Partition.Name is not null)
        {
            partition.Name = command.Partition.Name.Value;
        }

        if (command.Partition.Description is not null)
        {
            partition.Description = command.Partition.Description.Value;
        }

        await unitOfWork.SaveChanges(cancellationToken);

        await eventPublisher.PublishPartitionUpdated(partition.Guid, cancellationToken);
    }
}
