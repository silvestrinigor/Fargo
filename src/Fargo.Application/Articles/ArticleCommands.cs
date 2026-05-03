using Fargo.Application.Authentication;
using Fargo.Application.Partitions;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Barcodes;
using Fargo.Domain.Partitions;

namespace Fargo.Application.Articles;

#region Create

public sealed record ArticleCreateCommand(
    ArticleCreateDto Article
) : ICommand<Guid>;

public sealed class ArticleCreateCommandHandler(
    ActorService actorService,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork
    ) : ICommandHandler<ArticleCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

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
            article.Metrics = new ArticleMetrics
            {
                Mass = metrics.Mass,
                LengthX = metrics.LengthX,
                LengthY = metrics.LengthY,
                LengthZ = metrics.LengthZ
            };
        }

        #endregion Metrics

        #region Barcode

        if (command.Article.Barcodes is { } barcodes)
        {
            if (barcodes.Ean13 is { } ean13 && await articleRepository.ExistsByBarcode(ean13))
            {
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Ean13, ean13);
            }

            if (barcodes.Ean8 is { } ean8 && await articleRepository.ExistsByBarcode(ean8))
            {
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Ean8, ean8);
            }

            if (barcodes.UpcA is { } upcA && await articleRepository.ExistsByBarcode(upcA))
            {
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.UpcA, upcA);
            }

            if (barcodes.UpcE is { } upcE && await articleRepository.ExistsByBarcode(upcE))
            {
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.UpcE, upcE);
            }

            if (barcodes.Code128 is { } code128 && await articleRepository.ExistsByBarcode(code128))
            {
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Code128, code128);
            }

            if (barcodes.Code39 is { } code39 && await articleRepository.ExistsByBarcode(code39))
            {
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Code39, code39);
            }

            if (barcodes.Itf14 is { } itf14 && await articleRepository.ExistsByBarcode(itf14))
            {
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Itf14, itf14);
            }

            if (barcodes.Gs1128 is { } gs1128 && await articleRepository.ExistsByBarcode(gs1128))
            {
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Gs1128, gs1128);
            }

            if (barcodes.QrCode is { } qrCode && await articleRepository.ExistsByBarcode(qrCode))
            {
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.QrCode, qrCode);
            }

            if (barcodes.DataMatrix is { } dataMatrix && await articleRepository.ExistsByBarcode(dataMatrix))
            {
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.DataMatrix, dataMatrix);
            }

            article.Barcodes = new ArticleBarcodes
            {
                Ean13 = barcodes.Ean13,
                Ean8 = barcodes.Ean8,
                UpcA = barcodes.UpcA,
                UpcE = barcodes.UpcE,
                Code128 = barcodes.Code128,
                Code39 = barcodes.Code39,
                Itf14 = barcodes.Itf14,
                Gs1128 = barcodes.Gs1128,
                QrCode = barcodes.QrCode,
                DataMatrix = barcodes.DataMatrix
            };
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

        return article.Guid;
    }
}

#endregion Create

#region Delete

public sealed record ArticleDeleteCommand(
    Guid ArticleGuid
) : ICommand;

public sealed class ArticleDeleteCommandHandler(
    ActorService actorService,
    IArticleRepository articleRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<ArticleDeleteCommand>
{
    public async Task Handle(
        ArticleDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeleteArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        var hasItems = await articleRepository.HasItemsAssociated(
            article.Guid,
            cancellationToken
        );

        if (hasItems)
        {
            throw new ArticleDeleteWithItemsAssociatedFargoDomainException(article.Guid);
        }

        articleRepository.Remove(article);

        await unitOfWork.SaveChanges(cancellationToken);
    }
}

#endregion Delete

#region Update

public sealed record ArticleUpdateCommand(
    Guid ArticleGuid,
    ArticleUpdateDto Article
) : ICommand;

public sealed class ArticleUpdateCommandHandler(
    ActorService actorService,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser
) : ICommandHandler<ArticleUpdateCommand>
{
    public async Task Handle(
        ArticleUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

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

        #region Metrics

        if (command.Article.Metrics is { } metrics)
        {
            if (!article.Metrics.Mass.Equals(metrics.Mass))
            {
                article.Metrics.Mass = metrics.Mass;
            }

            if (!article.Metrics.LengthX.Equals(metrics.LengthX))
            {
                article.Metrics.LengthX = metrics.LengthX;
            }

            if (!article.Metrics.LengthY.Equals(metrics.LengthY))
            {
                article.Metrics.LengthY = metrics.LengthY;
            }

            if (!article.Metrics.LengthZ.Equals(metrics.LengthZ))
            {
                article.Metrics.LengthZ = metrics.LengthZ;
            }
        }

        #endregion Metrics

        #region Barcode

        if (command.Article.Barcodes is { } barcodes)
        {
            if (barcodes.Ean13 != article.Barcodes.Ean13)
            {
                if (barcodes.Ean13 is { } ean13 && await articleRepository.ExistsByBarcode(ean13))
                {
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Ean13, ean13);
                }

                article.Barcodes.Ean13 = barcodes.Ean13;
            }

            if (barcodes.Ean8 != article.Barcodes.Ean8)
            {
                if (barcodes.Ean8 is { } ean8 && await articleRepository.ExistsByBarcode(ean8))
                {
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Ean8, ean8);
                }

                article.Barcodes.Ean8 = barcodes.Ean8;
            }

            if (barcodes.UpcA != article.Barcodes.UpcA)
            {
                if (barcodes.UpcA is { } upcA && await articleRepository.ExistsByBarcode(upcA))
                {
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.UpcA, upcA);
                }

                article.Barcodes.UpcA = barcodes.UpcA;
            }

            if (barcodes.UpcE != article.Barcodes.UpcE)
            {
                if (barcodes.UpcE is { } upcE && await articleRepository.ExistsByBarcode(upcE))
                {
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.UpcE, upcE);
                }

                article.Barcodes.UpcE = barcodes.UpcE;
            }

            if (barcodes.Code128 != article.Barcodes.Code128)
            {
                if (barcodes.Code128 is { } code128 && await articleRepository.ExistsByBarcode(code128))
                {
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Code128, code128);
                }

                article.Barcodes.Code128 = barcodes.Code128;
            }

            if (barcodes.Code39 != article.Barcodes.Code39)
            {
                if (barcodes.Code39 is { } code39 && await articleRepository.ExistsByBarcode(code39))
                {
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Code39, code39);
                }

                article.Barcodes.Code39 = barcodes.Code39;
            }

            if (barcodes.Itf14 != article.Barcodes.Itf14)
            {
                if (barcodes.Itf14 is { } itf14 && await articleRepository.ExistsByBarcode(itf14))
                {
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Itf14, itf14);
                }

                article.Barcodes.Itf14 = barcodes.Itf14;
            }

            if (barcodes.Gs1128 != article.Barcodes.Gs1128)
            {
                if (barcodes.Gs1128 is { } gs1128 && await articleRepository.ExistsByBarcode(gs1128))
                {
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Gs1128, gs1128);
                }

                article.Barcodes.Gs1128 = barcodes.Gs1128;
            }

            if (barcodes.QrCode != article.Barcodes.QrCode)
            {
                if (barcodes.QrCode is { } qrCode && await articleRepository.ExistsByBarcode(qrCode))
                {
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.QrCode, qrCode);
                }

                article.Barcodes.QrCode = barcodes.QrCode;
            }

            if (barcodes.DataMatrix != article.Barcodes.DataMatrix)
            {
                if (barcodes.DataMatrix is { } dataMatrix && await articleRepository.ExistsByBarcode(dataMatrix))
                {
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.DataMatrix, dataMatrix);
                }

                article.Barcodes.DataMatrix = barcodes.DataMatrix;
            }
        }

        #endregion Barcode

        #region Partition

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

        #endregion Partition

        await unitOfWork.SaveChanges(cancellationToken);
    }
}

#endregion Update
