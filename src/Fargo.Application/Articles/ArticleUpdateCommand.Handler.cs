using Fargo.Application.Actors;
using Fargo.Application.Entities;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

public sealed class ArticlePatchCommandHandler(
    ActorQueryService actorService,
    ArticleService articleService,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor,
    IUnitOfWork unitOfWork,
    ILogger<ArticlePatchCommandHandler> logger
) : ICommandHandler<ArticleUpdateCommand>
{
    public async Task HandleAsync(
        ArticleUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.UpdateStarted(command.ArticleGuid, currentActor.ActorId);

        var articleUpdateDto = command.Article;

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.EditArticle);

        var article = await articleRepository.GetByGuidAsync(command.ArticleGuid, cancellationToken);

        EntityAssertFound.ThrowNotFoundIfNull(article);

        article.Name = articleUpdateDto.Name ?? article.Name;

        article.Description = articleUpdateDto.Description ?? article.Description;

        article.ShelfLife = articleUpdateDto.RemoveShelfLife is true ? null : articleUpdateDto.ShelfLife ?? article.ShelfLife;

        article.SetMetrics(
            articleUpdateDto.RemoveMass is true ? null : articleUpdateDto.Mass ?? article.Mass,
            articleUpdateDto.RemoveLengthX is true ? null : articleUpdateDto.LengthX ?? article.LengthX,
            articleUpdateDto.RemoveLengthY is true ? null : articleUpdateDto.LengthY ?? article.LengthY,
            articleUpdateDto.RemoveLengthZ is true ? null : articleUpdateDto.LengthZ ?? article.LengthZ);

        if (articleUpdateDto.RemoveEan13 is true)
        {
            article.Ean13 = null;
        }
        else if (articleUpdateDto.Ean13 is { } ean13)
        {
            await articleService.AssertArticleEan13IsAvailable(ean13, cancellationToken);

            article.Ean13 = ean13;
        }

        if (articleUpdateDto.RemoveEan8 is true)
        {
            article.Ean8 = null;
        }
        else if (articleUpdateDto.Ean8 is { } ean8)
        {
            await articleService.AssertArticleEan8IsAvailable(ean8, cancellationToken);

            article.Ean8 = ean8;
        }

        if (articleUpdateDto.RemoveUpcA is true)
        {
            article.UpcA = null;
        }
        else if (articleUpdateDto.UpcA is { } upcA)
        {
            await articleService.AssertArticleUpcAIsAvailable(upcA, cancellationToken);

            article.UpcA = upcA;
        }

        if (articleUpdateDto.RemoveUpcE is true)
        {
            article.UpcE = null;
        }
        else if (articleUpdateDto.UpcE is { } upcE)
        {
            await articleService.AssertArticleUpcEIsAvailable(upcE, cancellationToken);

            article.UpcE = upcE;
        }

        if (articleUpdateDto.RemoveCode128 is true)
        {
            article.Code128 = null;
        }
        else if (articleUpdateDto.Code128 is { } code128)
        {
            await articleService.AssertArticleCode128IsAvailable(code128, cancellationToken);

            article.Code128 = code128;
        }

        if (articleUpdateDto.RemoveCode39 is true)
        {
            article.Code39 = null;
        }
        else if (articleUpdateDto.Code39 is { } code39)
        {
            await articleService.AssertArticleCode39IsAvailable(code39, cancellationToken);

            article.Code39 = code39;
        }

        if (articleUpdateDto.RemoveItf14 is true)
        {
            article.Itf14 = null;
        }
        else if (articleUpdateDto.Itf14 is { } itf14)
        {
            await articleService.AssertArticleItf14IsAvailable(itf14, cancellationToken);

            article.Itf14 = itf14;
        }

        if (articleUpdateDto.RemoveGs1128 is true)
        {
            article.Gs1128 = null;
        }
        else if (articleUpdateDto.Gs1128 is { } gs1128)
        {
            await articleService.AssertArticleGs1128IsAvailable(gs1128, cancellationToken);

            article.Gs1128 = gs1128;
        }

        if (articleUpdateDto.RemoveQrCode is true)
        {
            article.QrCode = null;
        }
        else if (articleUpdateDto.QrCode is { } qrCode)
        {
            await articleService.AssertArticleQrCodeIsAvailable(qrCode, cancellationToken);

            article.QrCode = qrCode;
        }

        if (articleUpdateDto.RemoveDataMatrix is true)
        {
            article.DataMatrix = null;
        }
        if (articleUpdateDto.DataMatrix is { } dataMatrix)
        {
            await articleService.AssertArticleDataMatrixIsAvailable(dataMatrix, cancellationToken);

            article.DataMatrix = dataMatrix;
        }

        if (articleUpdateDto.PartitionsToAdd is { Count: > 0 } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuid(partitionGuid, cancellationToken);

                EntityAssertFound.ThrowNotFoundIfNull(partition);

                actor.ThrowIfAccessNotAuthorized(partition);

                article.AddPartition(partition);
            }
        }

        if (articleUpdateDto.PartitionsToRemove is { Count: > 0 } partitionsToRemove)
        {
            foreach (var partitionGuid in partitionsToRemove.Distinct())
            {
                var partition = await partitionRepository.GetByGuid(partitionGuid, cancellationToken);

                EntityAssertFound.ThrowNotFoundIfNull(partition);

                actor.ThrowIfAccessNotAuthorized(partition);

                article.RemovePartition(partition);
            }
        }

        article.IsActive = articleUpdateDto.IsActive ?? article.IsActive;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UpdateCompleted(article.Guid, actor.ActorId);
    }
}
