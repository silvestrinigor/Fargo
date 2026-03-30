using Fargo.Application.Exceptions;
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
/// This value must satisfy the validation rules defined in <see cref="Name"/>.
/// </param>
/// <param name="Description">
/// The optional description of the partition.
/// When not provided, <see cref="Description.Empty"/> is used.
/// </param>
/// <param name="ParentPartitionGuid">
/// The optional identifier of the parent partition.
/// When not provided, the new partition is created under the global partition.
/// </param>
/// <remarks>
/// Partitions form a hierarchical structure used to control access boundaries.
/// The created partition will be attached to a parent partition, either explicitly
/// provided or implicitly resolved to the global partition.
/// </remarks>
public sealed record PartitionCreateCommand(
        Name Name,
        Description? Description = null,
        Guid? ParentPartitionGuid = null
        ) : ICommand<Guid>;

/// <summary>
/// Handles the execution of <see cref="PartitionCreateCommand"/>.
/// </summary>
/// <remarks>
/// This handler is responsible for:
/// <list type="bullet">
/// <item><description>Resolving and authorizing the current actor</description></item>
/// <item><description>Validating permission to create partitions</description></item>
/// <item><description>Resolving the parent partition (or falling back to global)</description></item>
/// <item><description>Validating access to the parent partition</description></item>
/// <item><description>Delegating hierarchy rules to <see cref="PartitionService"/></description></item>
/// <item><description>Persisting the new partition</description></item>
/// </list>
///
/// Hierarchy rules:
/// <list type="bullet">
/// <item><description>
/// The parent-child relationship is managed by <see cref="PartitionService"/>
/// to enforce domain invariants such as preventing circular hierarchies
/// </description></item>
/// <item><description>
/// Direct manipulation of the hierarchy is not allowed outside the domain
/// </description></item>
/// </list>
///
/// Authorization rules:
/// <list type="bullet">
/// <item><description>The actor must have <see cref="ActionType.CreatePartition"/> permission</description></item>
/// <item><description>The actor must have access to the parent partition</description></item>
/// </list>
/// </remarks>
public sealed class PartitionCreateCommandHandler(
        ActorService actorService,
        PartitionService partitionService,
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
    /// <exception cref="UnauthorizedAccessFargoApplicationException">
    /// Thrown when the current user cannot be resolved or is not authorized.
    /// </exception>
    /// <exception cref="PartitionNotFoundFargoApplicationException">
    /// Thrown when the specified or resolved parent partition does not exist.
    /// </exception>
    /// <exception cref="UserNotAuthorizedFargoApplicationException">
    /// Thrown when the user does not have permission to create items.
    /// </exception>
    /// <remarks>
    /// Execution flow:
    /// <list type="number">
    /// <item><description>Resolve the current actor</description></item>
    /// <item><description>Validate <see cref="ActionType.CreatePartition"/> permission</description></item>
    /// <item><description>Resolve the parent partition (or fallback to global)</description></item>
    /// <item><description>Validate access to the parent partition</description></item>
    /// <item><description>Apply hierarchy rules via <see cref="PartitionService"/></description></item>
    /// <item><description>Persist the partition</description></item>
    /// </list>
    /// </remarks>
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

        var parentPartitionGuid = command.ParentPartitionGuid ?? PartitionService.GlobalPartitionGuid;

        var parentPartition = await partitionRepository.GetFoundByGuid(parentPartitionGuid, cancellationToken);

        actor.ValidateHasPartitionAccess(parentPartition.Guid);

        await partitionService.SetParentPartition(parentPartition, partition, cancellationToken);

        partitionRepository.Add(partition);

        await unitOfWork.SaveChanges(cancellationToken);

        return partition.Guid;
    }
}
