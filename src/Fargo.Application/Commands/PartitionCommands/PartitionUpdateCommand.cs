using Fargo.Application.Exceptions;
using Fargo.Application.Extensions;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Persistence;
using Fargo.Application.Security;
using Fargo.Domain.Enums;
using Fargo.Domain.Exceptions;
using Fargo.Domain.Repositories;
using Fargo.Domain.Services;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Commands.PartitionCommands;

/// <summary>
/// Command used to update an existing partition.
/// </summary>
/// <param name="PartitionGuid">
/// The unique identifier of the partition to update.
/// </param>
/// <param name="Partition">
/// The data containing the fields to be updated in the partition.
/// Only the properties provided in <see cref="PartitionUpdateModel"/> will be modified.
/// </param>
public sealed record PartitionUpdateCommand(
        Guid PartitionGuid,
        PartitionUpdateModel Partition
        ) : ICommand;

/// <summary>
/// Handles the execution of <see cref="PartitionUpdateCommand"/>.
/// </summary>
public sealed class PartitionUpdateCommandHandler(
        ActorService actorService,
        PartitionService partitionService,
        IPartitionRepository partitionRepository,
        IUserRepository userRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork
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
    /// Thrown when the specified partition does not exist or the actor
    /// does not have access to it.
    /// </exception>
    public async Task Handle(PartitionUpdateCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHassPermission(ActionType.EditPartition);

        var partition = await partitionRepository.GetFoundByGuid(command.PartitionGuid, cancellationToken);

        actor.ValidateHassPartitionAccess(partition.Guid);

        partition.Description = command.Partition.Description ?? Description.Empty;

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
