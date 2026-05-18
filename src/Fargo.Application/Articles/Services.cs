using Fargo.Core.Articles;

namespace Fargo.Application.Articles;

/// <summary>
/// Provides application operations for articles.
/// </summary>
/// <remarks>
/// Coordinates article commands and persists changes
/// using the unit of work.
/// </remarks>
public sealed class ArticleApplicationService(
    ICommandHandler<ArticleCreateCommand, Guid> createArticleHandler,
    ICommandHandler<ArticleCreateVariationCommand, Guid> createVariationHandler,
    ICommandHandler<ArticleCreatePackCommand, Guid> createPackHandler,
    ICommandHandler<ArticleCreateKitCommand, Guid> createKitHandler,
    ICommandHandler<ArticleCreateContainerCommand, Guid> createContainerHandler,
    ICommandHandler<ArticleSetContainerMaxMassCommand> setContainerMaxMassHandler,
    ICommandHandler<ArticleChangeDescriptionCommand> changeDescriptionHandler,
    ICommandHandler<ArticleSetShelfLifeCommand> setShelfLifeHandler,
    ICommandHandler<ArticleSetColorCommand> setColorHandler,
    ICommandHandler<ArticleSetMetricsCommand> setMetricsHandler,
    ICommandHandler<ArticleSetBarcodesCommand> setBarcodesHandler,
    ICommandHandler<ArticleSetPartitionsCommand> setPartitionsHandler,
    ICommandHandler<ArticleActivateCommand> activateHandler,
    ICommandHandler<ArticleDeactivateCommand> deactivateHandler,
    ICommandHandler<ArticleRenameCommand> renameHandler,
    ICommandHandler<ArticleDeleteCommand> deleteHandler,
    IUnitOfWork unitOfWork
)
{
    /// <summary>
    /// Creates a new article.
    /// </summary>
    /// <param name="create">
    /// Article creation data.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// The created article identifier.
    /// </returns>
    public async Task<Guid> Create(
        ArticleCreateDto create,
        CancellationToken cancellationToken = default)
    {
        var createTypeCount =
            (create.Variation is null ? 0 : 1) +
            (create.Pack is null ? 0 : 1) +
            (create.Kit is null ? 0 : 1) +
            (create.Container is null ? 0 : 1);

        if (createTypeCount > 1)
        {
            throw new ArgumentException(
                "Article creation accepts only one specialized article type.",
                nameof(create));
        }

        var articleGuid = create switch
        {
            { Variation: { } variation } => await createVariationHandler.Handle(
                new ArticleCreateVariationCommand(
                    create.Name,
                    variation.FromArticleGuid),
                cancellationToken),
            { Pack: { } pack } => await createPackHandler.Handle(
                new ArticleCreatePackCommand(
                    create.Name,
                    pack.FromArticleGuid,
                    pack.Quantity),
                cancellationToken),
            { Kit: { } kit } => await createKitHandler.Handle(
                new ArticleCreateKitCommand(
                    create.Name,
                    kit.Packs is { Count: > 0 } packs
                        ? packs
                            .Select(static pack => new ArticleKitComponentRequest(pack.ArticleGuid, pack.Quantity))
                            .ToArray()
                        : throw new ArgumentException(
                            "Kit article creation requires at least one pack.",
                            nameof(create))),
                cancellationToken),
            { Container: not null } => await createContainerHandler.Handle(
                new ArticleCreateContainerCommand(create.Name),
                cancellationToken),
            _ => await createArticleHandler.Handle(
                new ArticleCreateCommand(Guid.NewGuid(), create.Name),
                cancellationToken)
        };

        if (create.Description is { } description)
        {
            await changeDescriptionHandler.Handle(
                new ArticleChangeDescriptionCommand(articleGuid, description),
                cancellationToken);
        }

        if (create.ShelfLife is not null)
        {
            await setShelfLifeHandler.Handle(
                new ArticleSetShelfLifeCommand(articleGuid, create.ShelfLife),
                cancellationToken);
        }

        if (create.Color is not null)
        {
            await setColorHandler.Handle(
                new ArticleSetColorCommand(articleGuid, create.Color),
                cancellationToken);
        }

        if (create.Metrics is { } metrics)
        {
            await setMetricsHandler.Handle(
                new ArticleSetMetricsCommand(articleGuid, metrics.ToCore()),
                cancellationToken);
        }

        if (create.Barcodes is { } barcodes)
        {
            await setBarcodesHandler.Handle(
                new ArticleSetBarcodesCommand(articleGuid, barcodes.ToCore()),
                cancellationToken);
        }

        if (create.Container?.MaxMass is { } containerMaxMass)
        {
            await setContainerMaxMassHandler.Handle(
                new ArticleSetContainerMaxMassCommand(articleGuid, containerMaxMass),
                cancellationToken);
        }

        if (create.Partitions is { Count: > 0 } partitions)
        {
            await setPartitionsHandler.Handle(
                new ArticleSetPartitionsCommand(articleGuid, partitions),
                cancellationToken);
        }

        if (create.IsActive == false)
        {
            await deactivateHandler.Handle(new ArticleDeactivateCommand(articleGuid), cancellationToken);
        }

        await unitOfWork.SaveChanges(cancellationToken);

        return articleGuid;
    }

    /// <summary>
    /// Updates an existing article.
    /// </summary>
    /// <param name="articleGuid">
    /// Article unique identifier.
    /// </param>
    /// <param name="patch">
    /// Article update data.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    public async Task Patch(
        Guid articleGuid,
        ArticlePatchDto patch,
        CancellationToken cancellationToken = default)
    {
        if (patch.Name is { } name)
        {
            await renameHandler.Handle(
                new ArticleRenameCommand(articleGuid, name),
                cancellationToken);
        }

        if (patch.Description is { } description)
        {
            await changeDescriptionHandler.Handle(
                new ArticleChangeDescriptionCommand(articleGuid, description),
                cancellationToken);
        }

        if (patch.ShelfLife.IsSpecified)
        {
            await setShelfLifeHandler.Handle(
                new ArticleSetShelfLifeCommand(articleGuid, patch.ShelfLife.Value),
                cancellationToken);
        }

        if (patch.Metrics is { } metrics)
        {
            await setMetricsHandler.Handle(
                new ArticleSetMetricsCommand(
                    articleGuid,
                    metrics.ToCore()),
                cancellationToken);
        }

        if (patch.Barcodes is { } barcodes)
        {
            await setBarcodesHandler.Handle(
                new ArticleSetBarcodesCommand(articleGuid, barcodes.ToCore()),
                cancellationToken);
        }

        if (patch.Partitions is { } partitions)
        {
            await setPartitionsHandler.Handle(
                new ArticleSetPartitionsCommand(articleGuid, partitions),
                cancellationToken);
        }

        if (patch.IsActive is { } isActive)
        {
            if (isActive)
            {
                await activateHandler.Handle(new ArticleActivateCommand(articleGuid), cancellationToken);
            }
            else
            {
                await deactivateHandler.Handle(new ArticleDeactivateCommand(articleGuid), cancellationToken);
            }
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }

    /// <summary>
    /// Deletes an article.
    /// </summary>
    /// <param name="articleGuid">
    /// Article unique identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    public async Task Delete(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        await deleteHandler.Handle(new ArticleDeleteCommand(articleGuid), cancellationToken);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
