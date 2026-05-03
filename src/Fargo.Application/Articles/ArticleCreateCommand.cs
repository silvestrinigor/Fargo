using Fargo.Application.Authentication;
using Fargo.Application.Partitions;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Barcodes;
using Fargo.Domain.Partitions;

namespace Fargo.Application.Articles;

public sealed record ArticleCreateCommand(
    Name Name,
    Description? Description = null,
    TimeSpan? ShelfLife = null,
    ArticleMetricsDto? Metrics = null,
    ArticleBarcodesDto? Barcodes = null,
    IReadOnlyCollection<Guid>? PartitionGuids = null,
    bool? IsActive = null
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
        CancellationToken cancellationToken = default
    )
    {
        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateArticle);

        var article = new Article
        {
            Name = command.Name,
            Description = command.Description ?? Description.Empty,
            ShelfLife = command.ShelfLife
        };

        #region Metrics

        if (command.Metrics is { } metrics)
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

        if (command.Barcodes is { } barcodes)
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

        foreach (var partitionGuid in command.PartitionGuids ?? [])
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
