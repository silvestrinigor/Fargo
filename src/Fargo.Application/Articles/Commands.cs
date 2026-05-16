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

/// <summary>
/// Command used to create a new article.
/// </summary>
/// <param name="Name">
/// Article name.
/// </param>
public sealed record ArticleCreateCommand(
    Name Name
) : ICommand<Guid>;

/// <summary>
/// Handles article creation.
/// </summary>
/// <remarks>
/// Creates the article, validates permissions,
/// and stores the new entity.
/// </remarks>
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

#region Name

/// <summary>
/// Command used to rename an article.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="Name">
/// New article name.
/// </param>
public sealed record ArticleRenameCommand(
    Guid ArticleGuid,
    Name Name
) : ICommand;

/// <summary>
/// Handles article rename.
/// </summary>
/// <remarks>
/// Validates permissions and updates the article name.
/// </remarks>
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

#endregion Name

#region Description

/// <summary>
/// Command used to change the article description.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="Description">
/// New article description.
/// </param>
public sealed record ArticleChangeDescriptionCommand(
    Guid ArticleGuid,
    Description Description
) : ICommand;

/// <summary>
/// Handles article description changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates the description.
/// </remarks>
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

#endregion Description

#region ShelfLife

/// <summary>
/// Command used to define the article shelf life.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="ShelfLife">
/// Shelf life duration.
/// </param>
public sealed record ArticleSetShelfLifeCommand(
    Guid ArticleGuid,
    TimeSpan? ShelfLife
) : ICommand;


/// <summary>
/// Handles article shelf life changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates shelf life.
/// </remarks>
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

#endregion ShelfLife

#region Color

/// <summary>
/// Command used to define the article color.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="Color">
/// Article color.
/// </param>
public sealed record ArticleSetColorCommand(
    Guid ArticleGuid,
    Color? Color
) : ICommand;

/// <summary>
/// Handles article color changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates the color.
/// </remarks>
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

#endregion Color

#region Activate

/// <summary>
/// Command used to activate an article.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
public sealed record ArticleActivateCommand(
    Guid ArticleGuid
) : ICommand;

/// <summary>
/// Handles article activation.
/// </summary>
/// <remarks>
/// Validates permissions and activates the article.
/// </remarks>
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

/// <summary>
/// Command used to deactivate an article.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
public sealed record ArticleDeactivateCommand(
    Guid ArticleGuid
) : ICommand;

/// <summary>
/// Handles article deactivation.
/// </summary>
/// <remarks>
/// Validates permissions and deactivates the article.
/// </remarks>
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

        article.MarkModificationType(ArticleModifiedType.Deactivated);
    }
}

#endregion Deactivate

#region Metrics

/// <summary>
/// Command used to update article metrics.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="Metrics">
/// Article metrics information.
/// </param>
public sealed record ArticleSetMetricsCommand(
    Guid ArticleGuid,
    ArticleMetrics Metrics
) : ICommand;

/// <summary>
/// Handles article metrics changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates article metrics.
/// </remarks>
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

        article.SetMetrics(command.Metrics);

        article.MarkAsEditedBy(actor.ActorGuid);

        article.MarkModificationType(ArticleModifiedType.MetricsChanged);
    }
}

#endregion Metrics

#region Barcodes

/// <summary>
/// Command used to update article barcodes.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="Barcodes">
/// Article barcode information.
/// </param>
public sealed record ArticleSetBarcodesCommand(
    Guid ArticleGuid,
    ArticleBarcodesSet Barcodes
) : ICommand;

/// <summary>
/// Handles article barcode changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates article barcodes.
/// </remarks>
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

/// <summary>
/// Command used to update article partitions.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="PartitionGuids">
/// Collection of partition identifiers.
/// </param>
public sealed record ArticleSetPartitionsCommand(
    Guid ArticleGuid,
    IReadOnlyCollection<Guid> PartitionGuids
) : ICommand;

/// <summary>
/// Handles article partition changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates article partitions.
/// </remarks>
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

/// <summary>
/// Command used to delete an article.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
public sealed record ArticleDeleteCommand(
    Guid ArticleGuid
) : ICommand;

/// <summary>
/// Handles article deletion.
/// </summary>
/// <remarks>
/// Validates permissions and removes the article.
/// </remarks>
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
