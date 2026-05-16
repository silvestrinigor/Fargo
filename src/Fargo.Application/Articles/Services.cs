using Fargo.Core;
using System.Drawing;

namespace Fargo.Application.Articles;

public sealed class ArticleApplicationService(
    ICommandHandler<ArticleCreateCommand, Guid> createArticleHandler,
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
    public async Task<Guid> Create(
        Name name,
        Description? description = null,
        TimeSpan? shelfLife = null,
        Color? color = null,
        ArticleMetricsDto? metrics = null,
        ArticleBarcodesDto? barcodes = null,
        IReadOnlyCollection<Guid>? partitionGuids = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var articleGuid = await createArticleHandler.Handle(
            new ArticleCreateCommand(name),
            cancellationToken);

        if (description is { } descriptionValue)
        {
            await changeDescriptionHandler.Handle(
                new ArticleChangeDescriptionCommand(articleGuid, descriptionValue),
                cancellationToken);
        }

        if (shelfLife is not null)
        {
            await setShelfLifeHandler.Handle(
                new ArticleSetShelfLifeCommand(articleGuid, shelfLife),
                cancellationToken);
        }

        if (color is not null)
        {
            await setColorHandler.Handle(
                new ArticleSetColorCommand(articleGuid, color),
                cancellationToken);
        }

        if (metrics is { } metricsValue)
        {
            await setMetricsHandler.Handle(
                new ArticleSetMetricsCommand(articleGuid, metricsValue),
                cancellationToken);
        }

        if (barcodes is { } barcodesValue && HasAnyBarcode(barcodesValue))
        {
            await setBarcodesHandler.Handle(
                new ArticleSetBarcodesCommand(articleGuid, barcodesValue),
                cancellationToken);
        }

        if (partitionGuids is { Count: > 0 })
        {
            await setPartitionsHandler.Handle(
                new ArticleSetPartitionsCommand(articleGuid, partitionGuids),
                cancellationToken);
        }

        if (isActive == false)
        {
            await deactivateHandler.Handle(new ArticleDeactivateCommand(articleGuid), cancellationToken);
        }

        await unitOfWork.SaveChanges(cancellationToken);

        return articleGuid;
    }

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
                    metrics),
                cancellationToken);
        }

        if (HasAnyBarcodePatch(patch.Barcodes))
        {
            await setBarcodesHandler.Handle(
                new ArticleSetBarcodesCommand(
                    articleGuid,
                    await BuildBarcodesDto(articleGuid, patch.Barcodes, cancellationToken)),
                cancellationToken);
        }

        if (patch.Partitions.IsSpecified)
        {
            await setPartitionsHandler.Handle(
                new ArticleSetPartitionsCommand(articleGuid, patch.Partitions.Value ?? []),
                cancellationToken);
        }

        if (patch.IsActive.IsSpecified)
        {
            if (patch.IsActive.Value!.Value)
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

    public async Task Delete(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        await deleteHandler.Handle(new ArticleDeleteCommand(articleGuid), cancellationToken);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}
