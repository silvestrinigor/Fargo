namespace Fargo.Application.Items;

/// <summary>
/// Provides application operations for items.
/// </summary>
/// <remarks>
/// Coordinates item commands and persists changes
/// using the unit of work.
/// </remarks>
public sealed class ItemApplicationService(
    ICommandHandler<ItemCreateCommand, Guid> createHandler,
    ICommandHandler<ItemSetParentContainerCommand> setParentContainerHandler,
    ICommandHandler<ItemSetPartitionsCommand> setPartitionsHandler,
    ICommandHandler<ItemActivateCommand> activateHandler,
    ICommandHandler<ItemDeactivateCommand> deactivateHandler,
    ICommandHandler<ItemDeleteCommand> deleteHandler,
    IUnitOfWork unitOfWork)
{
    /// <summary>
    /// Creates a new item.
    /// </summary>
    /// <param name="create">
    /// Item creation data.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The created item identifier.
    /// </returns>
    public async Task<Guid> Create(
        ItemCreateDto create,
        CancellationToken cancellationToken = default)
    {
        var itemGuid = await createHandler.Handle(
            new ItemCreateCommand(Guid.NewGuid(), create.ArticleGuid, create.ProductionDate),
            cancellationToken);

        if (create.Partitions is { Count: > 0 } partitions)
        {
            await setPartitionsHandler.Handle(
                new ItemSetPartitionsCommand(itemGuid, partitions),
                cancellationToken);
        }

        if (create.IsActive == false)
        {
            await deactivateHandler.Handle(new ItemDeactivateCommand(itemGuid), cancellationToken);
        }

        await unitOfWork.SaveChanges(cancellationToken);

        return itemGuid;
    }

    /// <summary>
    /// Updates an existing item.
    /// </summary>
    /// <param name="itemGuid">
    /// Item unique identifier.
    /// </param>
    /// <param name="update">
    /// Item update data.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    public async Task Update(
        Guid itemGuid,
        ItemUpdateDto update,
        CancellationToken cancellationToken = default)
    {
        await setParentContainerHandler.Handle(
            new ItemSetParentContainerCommand(itemGuid, update.ParentContainerGuid),
            cancellationToken);

        await setPartitionsHandler.Handle(
            new ItemSetPartitionsCommand(itemGuid, update.Partitions),
            cancellationToken);

        if (update.IsActive is { } isActive)
        {
            if (isActive)
            {
                await activateHandler.Handle(new ItemActivateCommand(itemGuid), cancellationToken);
            }
            else
            {
                await deactivateHandler.Handle(new ItemDeactivateCommand(itemGuid), cancellationToken);
            }
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }

    /// <summary>
    /// Deletes an item.
    /// </summary>
    /// <param name="itemGuid">
    /// Item unique identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    public async Task Delete(
        Guid itemGuid,
        CancellationToken cancellationToken = default)
    {
        await deleteHandler.Handle(new ItemDeleteCommand(itemGuid), cancellationToken);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
