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
        var articleGuid = await CreateArticleByKind(create, cancellationToken);

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

        if (create.Kind == ArticleCreateKind.Container &&
            create.ContainerMaxMass is { } containerMaxMass)
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

    private async Task<Guid> CreateArticleByKind(
        ArticleCreateDto create,
        CancellationToken cancellationToken)
        => create.Kind switch
        {
            ArticleCreateKind.Article => await createArticleHandler.Handle(
                new ArticleCreateCommand(create.Name),
                cancellationToken),
            ArticleCreateKind.Variation => await createVariationHandler.Handle(
                new ArticleCreateVariationCommand(
                    create.Name,
                    create.FromArticleGuid ?? throw new ArgumentException(
                        "Variation article creation requires a source article guid.",
                        nameof(create))),
                cancellationToken),
            ArticleCreateKind.Pack => await createPackHandler.Handle(
                new ArticleCreatePackCommand(
                    create.Name,
                    create.FromArticleGuid ?? throw new ArgumentException(
                        "Pack article creation requires a source article guid.",
                        nameof(create)),
                    create.Quantity ?? throw new ArgumentException(
                        "Pack article creation requires a quantity.",
                        nameof(create))),
                cancellationToken),
            ArticleCreateKind.Kit => await createKitHandler.Handle(
                new ArticleCreateKitCommand(
                    create.Name,
                    create.KitPacks is { Count: > 0 } kitPacks
                        ? kitPacks
                            .Select(static pack => new ArticleKitComponent(pack.ArticleGuid, pack.Quantity))
                            .ToArray()
                        : throw new ArgumentException(
                            "Kit article creation requires at least one pack.",
                            nameof(create))),
                cancellationToken),
            ArticleCreateKind.Container => await createContainerHandler.Handle(
                new ArticleCreateContainerCommand(create.Name),
                cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(create), create.Kind, "Unsupported article create kind.")
        };

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
