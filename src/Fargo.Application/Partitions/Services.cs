using Fargo.Core.Partitions;

namespace Fargo.Application.Partitions;

/// <summary>
/// Provides application operations for partitions.
/// </summary>
/// <remarks>
/// Coordinates partition commands and persists changes
/// using the unit of work.
/// </remarks>
public sealed class PartitionApplicationService(
    ICommandHandler<PartitionCreateCommand, Guid> createHandler,
    ICommandHandler<PartitionRenameCommand> renameHandler,
    ICommandHandler<PartitionChangeDescriptionCommand> changeDescriptionHandler,
    ICommandHandler<PartitionSetParentCommand> setParentHandler,
    ICommandHandler<PartitionActivateCommand> activateHandler,
    ICommandHandler<PartitionDeactivateCommand> deactivateHandler,
    ICommandHandler<PartitionDeleteCommand> deleteHandler,
    IUnitOfWork unitOfWork)
{
    /// <summary>
    /// Creates a new partition.
    /// </summary>
    /// <param name="create">
    /// Partition creation data.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The created partition identifier.
    /// </returns>
    public async Task<Guid> Create(
        PartitionCreateDto create,
        CancellationToken cancellationToken = default)
    {
        var partitionGuid = await createHandler.Handle(
            new PartitionCreateCommand(create.Name),
            cancellationToken);

        if (create.Description is { } description)
        {
            await changeDescriptionHandler.Handle(
                new PartitionChangeDescriptionCommand(partitionGuid, description),
                cancellationToken);
        }

        await setParentHandler.Handle(
            new PartitionSetParentCommand(
                partitionGuid,
                create.ParentPartitionGuid ?? PartitionService.GlobalPartitionGuid),
            cancellationToken);

        await unitOfWork.SaveChanges(cancellationToken);

        return partitionGuid;
    }

    /// <summary>
    /// Updates an existing partition.
    /// </summary>
    /// <param name="partitionGuid">
    /// Partition unique identifier.
    /// </param>
    /// <param name="update">
    /// Partition update data.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    public async Task Update(
        Guid partitionGuid,
        PartitionUpdateDto update,
        CancellationToken cancellationToken = default)
    {
        if (update.Name is { } name)
        {
            await renameHandler.Handle(
                new PartitionRenameCommand(partitionGuid, name),
                cancellationToken);
        }

        if (update.Description is { } description)
        {
            await changeDescriptionHandler.Handle(
                new PartitionChangeDescriptionCommand(partitionGuid, description),
                cancellationToken);
        }

        if (update.ParentPartitionGuid is { } parentPartitionGuid)
        {
            await setParentHandler.Handle(
                new PartitionSetParentCommand(partitionGuid, parentPartitionGuid),
                cancellationToken);
        }

        if (update.IsActive is { } isActive)
        {
            if (isActive)
            {
                await activateHandler.Handle(new PartitionActivateCommand(partitionGuid), cancellationToken);
            }
            else
            {
                await deactivateHandler.Handle(new PartitionDeactivateCommand(partitionGuid), cancellationToken);
            }
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }

    /// <summary>
    /// Deletes a partition.
    /// </summary>
    /// <param name="partitionGuid">
    /// Partition unique identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    public async Task Delete(
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
    {
        await deleteHandler.Handle(new PartitionDeleteCommand(partitionGuid), cancellationToken);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
