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

    private static bool HasAnyBarcode(ArticleBarcodesDto barcodes)
        => barcodes.Ean13 is not null ||
           barcodes.Ean8 is not null ||
           barcodes.UpcA is not null ||
           barcodes.UpcE is not null ||
           barcodes.Code128 is not null ||
           barcodes.Code39 is not null ||
           barcodes.Itf14 is not null ||
           barcodes.Gs1128 is not null ||
           barcodes.QrCode is not null ||
           barcodes.DataMatrix is not null;

    public async Task Update(
        Guid articleGuid,
        Name name,
        Description description,
        TimeSpan? shelfLife,
        Color? color,
        ArticleMetricsDto metrics,
        ArticleBarcodesDto barcodes,
        IReadOnlyCollection<Guid> partitionGuids,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await renameHandler.Handle(
            new ArticleRenameCommand(articleGuid, name),
            cancellationToken);

        await changeDescriptionHandler.Handle(
            new ArticleChangeDescriptionCommand(articleGuid, description),
            cancellationToken);

        await setShelfLifeHandler.Handle(
            new ArticleSetShelfLifeCommand(articleGuid, shelfLife),
            cancellationToken);

        await setColorHandler.Handle(
            new ArticleSetColorCommand(articleGuid, color),
            cancellationToken);

        await setMetricsHandler.Handle(
            new ArticleSetMetricsCommand(articleGuid, metrics),
            cancellationToken);

        await setBarcodesHandler.Handle(
            new ArticleSetBarcodesCommand(articleGuid, barcodes),
            cancellationToken);

        await setPartitionsHandler.Handle(
            new ArticleSetPartitionsCommand(articleGuid, partitionGuids),
            cancellationToken);

        if (isActive)
        {
            await activateHandler.Handle(new ArticleActivateCommand(articleGuid), cancellationToken);
        }
        else
        {
            await deactivateHandler.Handle(new ArticleDeactivateCommand(articleGuid), cancellationToken);
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
