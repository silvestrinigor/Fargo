using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

public sealed class ArticlePatchCommandHandler(
    ActorService actorService, ArticleService articleService,
    IArticleRepository articleRepository, IPartitionRepository partitionRepository,
    ICurrentActor currentActor, IUnitOfWork unitOfWork,
    ILogger<ArticlePatchCommandHandler> logger
) : ICommandHandler<ArticleUpdateCommand>
{
    public async Task HandleAsync(
        ArticleUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.UpdateStarted(command.ArticleGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.EditArticle);

        var article = await articleRepository.GetByGuidAsync(command.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, command.ArticleGuid, EntityType.Article);

        actor.ThrowIfAccessDenied(article);

        var articleUpdateDto = command.Article;

        article.Name = articleUpdateDto.Name ?? article.Name;

        article.Description = articleUpdateDto.Description ?? article.Description;

        article.ShelfLife = articleUpdateDto.RemoveShelfLife is true
            ? null : articleUpdateDto.ShelfLife ?? article.ShelfLife;

        article.SetMetrics(
            articleUpdateDto.RemoveMass is true
                ? null : articleUpdateDto.Mass ?? article.Mass,

            articleUpdateDto.Dimension?.RemoveLengthX is true
                ? null : articleUpdateDto.Dimension?.LengthX ?? article.Dimension.X,

            articleUpdateDto.Dimension?.RemoveLengthY is true
                ? null : articleUpdateDto.Dimension?.LengthY ?? article.Dimension.Y,

            articleUpdateDto.Dimension?.RemoveLengthZ is true
                ? null : articleUpdateDto.Dimension?.LengthZ ?? article.Dimension.Z);

        if (articleUpdateDto.Barcode?.RemoveEan13 is true)
        {
            article.Barcode.Ean13 = null;
        }

        else if (articleUpdateDto.Barcode?.Ean13 is { } ean13)
        {
            await articleService.AssertArticleEan13IsAvailableAsync(ean13, cancellationToken);

            article.Barcode.Ean13 = ean13;
        }

        if (articleUpdateDto.Barcode?.RemoveEan8 is true)
        {
            article.Barcode.Ean8 = null;
        }

        else if (articleUpdateDto.Barcode?.Ean8 is { } ean8)
        {
            await articleService.AssertArticleEan8IsAvailableAsync(ean8, cancellationToken);

            article.Barcode.Ean8 = ean8;
        }

        if (articleUpdateDto.Barcode?.RemoveUpcA is true)
        {
            article.Barcode.UpcA = null;
        }

        else if (articleUpdateDto.Barcode?.UpcA is { } upcA)
        {
            await articleService.AssertArticleUpcAIsAvailableAsync(upcA, cancellationToken);

            article.Barcode.UpcA = upcA;
        }

        if (articleUpdateDto.Barcode?.RemoveUpcE is true)
        {
            article.Barcode.UpcE = null;
        }

        else if (articleUpdateDto.Barcode?.UpcE is { } upcE)
        {
            await articleService.AssertArticleUpcEIsAvailableAsync(upcE, cancellationToken);

            article.Barcode.UpcE = upcE;
        }

        if (articleUpdateDto.Barcode?.RemoveCode128 is true)
        {
            article.Barcode.Code128 = null;
        }

        else if (articleUpdateDto.Barcode?.Code128 is { } code128)
        {
            await articleService.AssertArticleCode128IsAvailableAsync(code128, cancellationToken);

            article.Barcode.Code128 = code128;
        }

        if (articleUpdateDto.Barcode?.RemoveCode39 is true)
        {
            article.Barcode.Code39 = null;
        }

        else if (articleUpdateDto.Barcode?.Code39 is { } code39)
        {
            await articleService.AssertArticleCode39IsAvailableAsync(code39, cancellationToken);

            article.Barcode.Code39 = code39;
        }

        if (articleUpdateDto.Barcode?.RemoveItf14 is true)
        {
            article.Barcode.Itf14 = null;
        }

        else if (articleUpdateDto.Barcode?.Itf14 is { } itf14)
        {
            await articleService.AssertArticleItf14IsAvailableAsync(itf14, cancellationToken);

            article.Barcode.Itf14 = itf14;
        }

        if (articleUpdateDto.Barcode?.RemoveGs1128 is true)
        {
            article.Barcode.Gs1128 = null;
        }

        else if (articleUpdateDto.Barcode?.Gs1128 is { } gs1128)
        {
            await articleService.AssertArticleGs1128IsAvailableAsync(gs1128, cancellationToken);

            article.Barcode.Gs1128 = gs1128;
        }

        if (articleUpdateDto.Barcode?.RemoveQrCode is true)
        {
            article.Barcode.QrCode = null;
        }

        else if (articleUpdateDto.Barcode?.QrCode is { } qrCode)
        {
            await articleService.AssertArticleQrCodeIsAvailableAsync(qrCode, cancellationToken);

            article.Barcode.QrCode = qrCode;
        }

        if (articleUpdateDto.Barcode?.RemoveDataMatrix is true)
        {
            article.Barcode.DataMatrix = null;
        }

        else if (articleUpdateDto.Barcode?.DataMatrix is { } dataMatrix)
        {
            await articleService.AssertArticleDataMatrixIsAvailableAsync(dataMatrix, cancellationToken);

            article.Barcode.DataMatrix = dataMatrix;
        }

        if (articleUpdateDto.PartitionsToAdd is { Count: > 0 } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                article.AddPartition(partition);
            }
        }

        if (articleUpdateDto.PartitionsToRemove is { Count: > 0 } partitionsToRemove)
        {
            foreach (var partitionGuid in partitionsToRemove.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                article.RemovePartition(partition);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UpdateCompleted(article.Guid, actor.Guid);
    }
}
