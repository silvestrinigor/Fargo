using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;
using System.Drawing;

namespace Fargo.Application.Articles;

#region Create

public sealed record ArticleCreateCommand(
    Name Name
) : ICommand<Guid>;

public sealed class ArticleCreateCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleCreateCommandHandler> logger
) : ICommandHandler<ArticleCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Article create flow started for actor {ActorGuid}.", actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.CreateArticle);

        var article = new Article(command.Name);

        article.MarkAsEditedBy(actor.ActorGuid);

        articleRepository.Add(article);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article create mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.ActorGuid);
        }

        return article.Guid;
    }
}

#endregion Create

#region General

public sealed record ArticleRenameCommand(
    Guid ArticleGuid,
    Name Name
) : ICommand;

public sealed class ArticleRenameCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext
) : ICommandHandler<ArticleRenameCommand>
{
    public async Task Handle(
        ArticleRenameCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        article.StartEdit(actor.ToActor());

        article.Rename(command.Name);
    }
}

public sealed record ArticleChangeDescriptionCommand(
    Guid ArticleGuid,
    Description Description
) : ICommand;

public sealed class ArticleChangeDescriptionCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext
) : ICommandHandler<ArticleChangeDescriptionCommand>
{
    public async Task Handle(
        ArticleChangeDescriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        article.StartEdit(actor.ToActor());

        article.ChangeDescription(command.Description);
    }
}

public sealed record ArticleSetShelfLifeCommand(
    Guid ArticleGuid,
    TimeSpan? ShelfLife
) : ICommand;

public sealed class ArticleSetShelfLifeCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext
) : ICommandHandler<ArticleSetShelfLifeCommand>
{
    public async Task Handle(
        ArticleSetShelfLifeCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        article.StartEdit(actor.ToActor());

        article.SetShelfLife(command.ShelfLife);
    }
}

public sealed record ArticleSetColorCommand(
    Guid ArticleGuid,
    Color? Color
) : ICommand;

public sealed class ArticleSetColorCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext
) : ICommandHandler<ArticleSetColorCommand>
{
    public async Task Handle(
        ArticleSetColorCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        article.StartEdit(actor.ToActor());

        article.SetColor(command.Color);
    }
}

public sealed record ArticleActivateCommand(
    Guid ArticleGuid
) : ICommand;

public sealed class ArticleActivateCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext
) : ICommandHandler<ArticleActivateCommand>
{
    public async Task Handle(
        ArticleActivateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        article.StartEdit(actor.ToActor());

        article.Activate();
    }
}

public sealed record ArticleDeactivateCommand(
    Guid ArticleGuid
) : ICommand;

public sealed class ArticleDeactivateCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext
) : ICommandHandler<ArticleDeactivateCommand>
{
    public async Task Handle(
        ArticleDeactivateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        article.StartEdit(actor.ToActor());

        article.Deactivate();
    }
}

#endregion General

#region Metrics

public sealed record ArticleSetMetricsCommand(
    Guid ArticleGuid,
    ArticleMetricsDto Metrics
) : ICommand;

public sealed class ArticleSetMetricsCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext
) : ICommandHandler<ArticleSetMetricsCommand>
{
    public async Task Handle(
        ArticleSetMetricsCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        article.StartEdit(actor.ToActor());

        article.SetMetrics(
            command.Metrics.Mass,
            command.Metrics.LengthX,
            command.Metrics.LengthY,
            command.Metrics.LengthZ);
    }
}

#endregion Metrics

#region Barcodes

public sealed record ArticleSetBarcodesCommand(
    Guid ArticleGuid,
    ArticleBarcodesDto Barcodes
) : ICommand;

public sealed class ArticleSetBarcodesCommandHandler(
    IArticleRepository articleRepository,
    ArticleService articleService,
    ICurrentAuthorizationContext currentAuthorizationContext
) : ICommandHandler<ArticleSetBarcodesCommand>
{
    public async Task Handle(
        ArticleSetBarcodesCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        article.StartEdit(actor.ToActor());

        ValidateBarcodePermissions(actor, article.Ean13, command.Barcodes.Ean13);

        ValidateBarcodePermissions(actor, article.Ean8, command.Barcodes.Ean8);

        ValidateBarcodePermissions(actor, article.UpcA, command.Barcodes.UpcA);

        ValidateBarcodePermissions(actor, article.UpcE, command.Barcodes.UpcE);

        ValidateBarcodePermissions(actor, article.Code128, command.Barcodes.Code128);

        ValidateBarcodePermissions(actor, article.Code39, command.Barcodes.Code39);

        ValidateBarcodePermissions(actor, article.Itf14, command.Barcodes.Itf14);

        ValidateBarcodePermissions(actor, article.Gs1128, command.Barcodes.Gs1128);

        ValidateBarcodePermissions(actor, article.QrCode, command.Barcodes.QrCode);

        ValidateBarcodePermissions(actor, article.DataMatrix, command.Barcodes.DataMatrix);

        await articleService.SetEan13(command.Barcodes.Ean13, article, cancellationToken);

        await articleService.SetEan8(command.Barcodes.Ean8, article, cancellationToken);

        await articleService.SetUpcA(command.Barcodes.UpcA, article, cancellationToken);

        await articleService.SetUpcE(command.Barcodes.UpcE, article, cancellationToken);

        await articleService.SetCode128(command.Barcodes.Code128, article, cancellationToken);

        await articleService.SetCode39(command.Barcodes.Code39, article, cancellationToken);

        await articleService.SetItf14(command.Barcodes.Itf14, article, cancellationToken);

        await articleService.SetGs1128(command.Barcodes.Gs1128, article, cancellationToken);

        await articleService.SetQrCode(command.Barcodes.QrCode, article, cancellationToken);

        await articleService.SetDataMatrix(command.Barcodes.DataMatrix, article, cancellationToken);
    }

    private static void ValidateBarcodePermissions<TBarcode>(
        IAuthorizationContext actor,
        TBarcode? current,
        TBarcode? requested)
        where TBarcode : struct, IEquatable<TBarcode>
    {
        if (EqualityComparer<TBarcode?>.Default.Equals(current, requested))
        {
            return;
        }

        if (current is null && requested is not null)
        {
            actor.ValidateHasPermission(ActionType.AddBarcode);
        }
        else if (current is not null && requested is null)
        {
            actor.ValidateHasPermission(ActionType.RemoveBarcode);
        }
        else
        {
            actor.ValidateHasPermission(ActionType.AddBarcode);
            actor.ValidateHasPermission(ActionType.RemoveBarcode);
        }
    }
}

#endregion Barcodes

#region Partitions

public sealed record ArticleSetPartitionsCommand(
    Guid ArticleGuid,
    IReadOnlyCollection<Guid> PartitionGuids
) : ICommand;

public sealed class ArticleSetPartitionsCommandHandler(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext
) : ICommandHandler<ArticleSetPartitionsCommand>
{
    public async Task Handle(
        ArticleSetPartitionsCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        article.StartEdit(actor.ToActor());

        foreach (var partitionGuid in command.PartitionGuids)
        {
            if (article.Partitions.Any(p => p.Guid == partitionGuid))
            {
                continue;
            }

            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);

            article.AddPartition(partition);
        }

        var partitionsToRemove = article.Partitions
            .Where(p => !command.PartitionGuids.Contains(p.Guid))
            .ToList();

        foreach (var partition in partitionsToRemove)
        {
            actor.ValidateHasPartitionAccess(partition.Guid);

            article.RemovePartition(partition);
        }
    }
}

#endregion Partitions

#region Delete

public sealed record ArticleDeleteCommand(
    Guid ArticleGuid
) : ICommand;

public sealed class ArticleDeleteCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleDeleteCommandHandler> logger
) : ICommandHandler<ArticleDeleteCommand>
{
    public async Task Handle(
        ArticleDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article delete flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.DeleteArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        var hasItems = await articleRepository.HasItemsAssociated(
            article.Guid,
            cancellationToken
        );

        if (hasItems)
        {
            logger.LogWarning(
                "Article delete flow rejected because article {ArticleGuid} has associated items.",
                article.Guid);
            throw new ArticleDeleteWithItemsAssociatedFargoDomainException(article.Guid);
        }

        articleRepository.Remove(article);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article delete mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Delete
