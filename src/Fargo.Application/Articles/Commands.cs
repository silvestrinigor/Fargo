using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
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

        var article = Article.CreateArticle(command.Name);

        article.MarkAsEditedBy(actor.ActorGuid);

        article.MarkModificationType(ArticleModifiedType.General);

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

        if (article.Name == command.Name)
        {
            return;
        }

        article.Rename(command.Name);

        article.MarkAsEditedBy(actor.ActorGuid);

        article.MarkModificationType(ArticleModifiedType.General);
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

        if (article.Description == command.Description)
        {
            return;
        }

        article.ChangeDescription(command.Description);

        article.MarkAsEditedBy(actor.ActorGuid);

        article.MarkModificationType(ArticleModifiedType.General);
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

        if (article.ShelfLife == command.ShelfLife)
        {
            return;
        }

        article.SetShelfLife(command.ShelfLife);

        article.MarkAsEditedBy(actor.ActorGuid);

        article.MarkModificationType(ArticleModifiedType.General);
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

        if (article.Color == command.Color)
        {
            return;
        }

        article.SetColor(command.Color);

        article.MarkAsEditedBy(actor.ActorGuid);

        article.MarkModificationType(ArticleModifiedType.General);
    }
}

#endregion General

#region Activate

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

        if (article.IsActive)
        {
            return;
        }

        article.Activate();

        article.MarkAsEditedBy(actor.ActorGuid);

        article.MarkModificationType(ArticleModifiedType.Activated);
    }
}

#endregion Activate

#region Deactivate

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

        if (!article.IsActive)
        {
            return;
        }

        article.Deactivate();

        article.MarkAsEditedBy(actor.ActorGuid);

        article.MarkModificationType(ArticleModifiedType.Deactivate);
    }
}

#endregion Deactivate

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

        if (Nullable.Equals(article.Mass, command.Metrics.Mass) &&
            Nullable.Equals(article.LengthX, command.Metrics.LengthX) &&
            Nullable.Equals(article.LengthY, command.Metrics.LengthY) &&
            Nullable.Equals(article.LengthZ, command.Metrics.LengthZ))
        {
            return;
        }

        article.SetMetrics(
            command.Metrics.Mass,
            command.Metrics.LengthX,
            command.Metrics.LengthY,
            command.Metrics.LengthZ);

        article.MarkAsEditedBy(actor.ActorGuid);

        article.MarkModificationType(ArticleModifiedType.MetricsChanged);
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

        var hasChanges =
            !EqualityComparer<Ean13?>.Default.Equals(article.Ean13, command.Barcodes.Ean13) ||
            !EqualityComparer<Ean8?>.Default.Equals(article.Ean8, command.Barcodes.Ean8) ||
            !EqualityComparer<UpcA?>.Default.Equals(article.UpcA, command.Barcodes.UpcA) ||
            !EqualityComparer<UpcE?>.Default.Equals(article.UpcE, command.Barcodes.UpcE) ||
            !EqualityComparer<Code128?>.Default.Equals(article.Code128, command.Barcodes.Code128) ||
            !EqualityComparer<Code39?>.Default.Equals(article.Code39, command.Barcodes.Code39) ||
            !EqualityComparer<Itf14?>.Default.Equals(article.Itf14, command.Barcodes.Itf14) ||
            !EqualityComparer<Gs1128?>.Default.Equals(article.Gs1128, command.Barcodes.Gs1128) ||
            !EqualityComparer<QrCode?>.Default.Equals(article.QrCode, command.Barcodes.QrCode) ||
            !EqualityComparer<DataMatrix?>.Default.Equals(article.DataMatrix, command.Barcodes.DataMatrix);

        if (!hasChanges)
        {
            return;
        }

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

        article.MarkAsEditedBy(actor.ActorGuid);

        article.MarkModificationType(ArticleModifiedType.BarcodesChanged);
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

        var requestedPartitionGuids = command.PartitionGuids.Distinct().ToArray();

        var hasChanges =
            article.Partitions.Count != requestedPartitionGuids.Length ||
            article.Partitions.Any(p => !requestedPartitionGuids.Contains(p.Guid));

        if (!hasChanges)
        {
            return;
        }

        foreach (var partitionGuid in requestedPartitionGuids)
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
            .Where(p => !requestedPartitionGuids.Contains(p.Guid))
            .ToList();

        foreach (var partition in partitionsToRemove)
        {
            actor.ValidateHasPartitionAccess(partition.Guid);

            article.RemovePartition(partition);
        }

        article.MarkAsEditedBy(actor.ActorGuid);

        article.MarkModificationType(ArticleModifiedType.PartitionsChanged);
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
