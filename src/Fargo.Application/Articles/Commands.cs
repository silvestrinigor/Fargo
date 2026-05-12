using Fargo.Application.Authentication;
using Fargo.Application.Partitions;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

#region Create

public sealed record ArticleCreateCommand(
    ArticleCreateDto Article
) : ICommand<Guid>;

public sealed class ArticleCreateCommandHandler(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
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

        var article = new Article
        {
            Name = command.Article.Name,
            Description = command.Article.Description ?? Description.Empty,
            ShelfLife = command.Article.ShelfLife
        };

        #region Metrics

        if (command.Article.Metrics is { } metrics)
        {
            article.Mass = metrics.Mass;
            article.LengthX = metrics.LengthX;
            article.LengthY = metrics.LengthY;
            article.LengthZ = metrics.LengthZ;
        }

        #endregion Metrics

        #region Barcode

        if (command.Article.Barcodes is { } barcodes)
        {
            if (barcodes.Ean13 is { } ean13 && await articleRepository.ExistsByBarcode(ean13))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.Ean13);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Ean13, ean13);
            }

            article.Ean13 = barcodes.Ean13;

            if (barcodes.Ean8 is { } ean8 && await articleRepository.ExistsByBarcode(ean8))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.Ean8);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Ean8, ean8);
            }

            article.Ean8 = barcodes.Ean8;

            if (barcodes.UpcA is { } upcA && await articleRepository.ExistsByBarcode(upcA))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.UpcA);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.UpcA, upcA);
            }

            article.UpcA = barcodes.UpcA;

            if (barcodes.UpcE is { } upcE && await articleRepository.ExistsByBarcode(upcE))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.UpcE);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.UpcE, upcE);
            }

            article.UpcE = barcodes.UpcE;

            if (barcodes.Code128 is { } code128 && await articleRepository.ExistsByBarcode(code128))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.Code128);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Code128, code128);
            }

            article.Code128 = barcodes.Code128;

            if (barcodes.Code39 is { } code39 && await articleRepository.ExistsByBarcode(code39))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.Code39);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Code39, code39);
            }

            article.Code39 = barcodes.Code39;

            if (barcodes.Itf14 is { } itf14 && await articleRepository.ExistsByBarcode(itf14))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.Itf14);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Itf14, itf14);
            }

            article.Itf14 = barcodes.Itf14;

            if (barcodes.Gs1128 is { } gs1128 && await articleRepository.ExistsByBarcode(gs1128))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.Gs1128);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Gs1128, gs1128);
            }

            article.Gs1128 = barcodes.Gs1128;

            if (barcodes.QrCode is { } qrCode && await articleRepository.ExistsByBarcode(qrCode))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.QrCode);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.QrCode, qrCode);
            }

            article.QrCode = barcodes.QrCode;

            if (barcodes.DataMatrix is { } dataMatrix && await articleRepository.ExistsByBarcode(dataMatrix))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.DataMatrix);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.DataMatrix, dataMatrix);
            }

            article.DataMatrix = barcodes.DataMatrix;
        }

        #endregion Barcode

        #region Partition

        foreach (var partitionGuid in command.Article.Partitions ?? [])
        {
            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);

            article.Partitions.Add(partition);
        }

        #endregion Partition

        articleRepository.Add(article);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article create flow completed for article {ArticleGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}.",
                article.Guid,
                actor.ActorGuid,
                article.Partitions.Count);
        }

        return article.Guid;
    }
}

#endregion Create

#region Delete

public sealed record ArticleDeleteCommand(
    Guid ArticleGuid
) : ICommand;

public sealed class ArticleDeleteCommandHandler(
    IArticleRepository articleRepository,
    IUnitOfWork unitOfWork,
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

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article delete flow completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Delete

#region Update

public sealed record ArticleUpdateCommand(
    Guid ArticleGuid,
    ArticleUpdateDto Article
) : ICommand;

public sealed class ArticleUpdateCommandHandler(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IUnitOfWork unitOfWork,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleUpdateCommandHandler> logger
) : ICommandHandler<ArticleUpdateCommand>
{
    public async Task Handle(
        ArticleUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article update flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        if (article.Name != command.Article.Name)
        {
            article.Name = command.Article.Name;
        }

        if (article.Description != command.Article.Description)
        {
            article.Description = command.Article.Description;
        }

        if (article.ShelfLife != command.Article.ShelfLife)
        {
            article.ShelfLife = command.Article.ShelfLife;
        }

        if (command.Article.Metrics is { } metrics)
        {
            if (!article.Mass.Equals(metrics.Mass))
            {
                article.Mass = metrics.Mass;
            }

            if (!article.LengthX.Equals(metrics.LengthX))
            {
                article.LengthX = metrics.LengthX;
            }

            if (!article.LengthY.Equals(metrics.LengthY))
            {
                article.LengthY = metrics.LengthY;
            }

            if (!article.LengthZ.Equals(metrics.LengthZ))
            {
                article.LengthZ = metrics.LengthZ;
            }
        }

        if (command.Article.Barcodes is { } barcodes)
        {
            if (barcodes.Ean13 != article.Ean13)
            {
                if (barcodes.Ean13 is { } ean13 && await articleRepository.ExistsByBarcode(ean13))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.Ean13, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Ean13, ean13);
                }

                article.Ean13 = barcodes.Ean13;
            }

            if (barcodes.Ean8 != article.Ean8)
            {
                if (barcodes.Ean8 is { } ean8 && await articleRepository.ExistsByBarcode(ean8))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.Ean8, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Ean8, ean8);
                }

                article.Ean8 = barcodes.Ean8;
            }

            if (barcodes.UpcA != article.UpcA)
            {
                if (barcodes.UpcA is { } upcA && await articleRepository.ExistsByBarcode(upcA))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.UpcA, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.UpcA, upcA);
                }

                article.UpcA = barcodes.UpcA;
            }

            if (barcodes.UpcE != article.UpcE)
            {
                if (barcodes.UpcE is { } upcE && await articleRepository.ExistsByBarcode(upcE))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.UpcE, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.UpcE, upcE);
                }

                article.UpcE = barcodes.UpcE;
            }

            if (barcodes.Code128 != article.Code128)
            {
                if (barcodes.Code128 is { } code128 && await articleRepository.ExistsByBarcode(code128))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.Code128, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Code128, code128);
                }

                article.Code128 = barcodes.Code128;
            }

            if (barcodes.Code39 != article.Code39)
            {
                if (barcodes.Code39 is { } code39 && await articleRepository.ExistsByBarcode(code39))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.Code39, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Code39, code39);
                }

                article.Code39 = barcodes.Code39;
            }

            if (barcodes.Itf14 != article.Itf14)
            {
                if (barcodes.Itf14 is { } itf14 && await articleRepository.ExistsByBarcode(itf14))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.Itf14, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Itf14, itf14);
                }

                article.Itf14 = barcodes.Itf14;
            }

            if (barcodes.Gs1128 != article.Gs1128)
            {
                if (barcodes.Gs1128 is { } gs1128 && await articleRepository.ExistsByBarcode(gs1128))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.Gs1128, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Gs1128, gs1128);
                }

                article.Gs1128 = barcodes.Gs1128;
            }

            if (barcodes.QrCode != article.QrCode)
            {
                if (barcodes.QrCode is { } qrCode && await articleRepository.ExistsByBarcode(qrCode))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.QrCode, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.QrCode, qrCode);
                }

                article.QrCode = barcodes.QrCode;
            }

            if (barcodes.DataMatrix != article.DataMatrix)
            {
                if (barcodes.DataMatrix is { } dataMatrix && await articleRepository.ExistsByBarcode(dataMatrix))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.DataMatrix, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.DataMatrix, dataMatrix);
                }

                article.DataMatrix = barcodes.DataMatrix;
            }
        }

        if (command.Article.Partitions is { } requestedPartitions)
        {
            foreach (var partitionGuid in requestedPartitions)
            {
                if (article.Partitions.Any(p => p.Guid == partitionGuid))
                {
                    continue;
                }

                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                actor.ValidateHasPartitionAccess(partition.Guid);

                article.Partitions.Add(partition);
            }

            var partitionsToRemove = article.Partitions
                .Where(p => !requestedPartitions.Contains(p.Guid))
                .ToList();

            foreach (var partition in partitionsToRemove)
            {
                actor.ValidateHasPartitionAccess(partition.Guid);

                article.Partitions.Remove(partition);
            }
        }

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article update flow completed for article {ArticleGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}.",
                article.Guid,
                actor.ActorGuid,
                article.Partitions.Count);
        }
    }
}

#endregion Update
