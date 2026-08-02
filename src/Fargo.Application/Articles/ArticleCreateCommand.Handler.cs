using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Articles;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Fargo.Application.Articles;

public sealed class ArticleCreateCommandHandler(
    ArticleService articleService, ActorService actorService,
    IArticleRepository articleRepository, IPartitionRepository partitionRepository,
    ICurrentActor currentActor, IUnitOfWork unitOfWork,
    ILogger<ArticleCreateCommandHandler> logger
    ) : ICommandHandler<ArticleCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        ArticleCreateCommand command, CancellationToken cancellationToken = default)
    {
        logger.CreateStarted(currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.ActorId);

        actor.ThrowIfPermissionDenied(ActionType.CreateArticle);

        Article article;

        switch (command.ArticleType)
        {
            case ArticleType.Default:
                article = Article.NewArticle(command.Name);
                break;

            case ArticleType.Variation:
                {
                    var fromArticle = await articleRepository.GetByGuidAsync(command.FromArticle!.Value, cancellationToken);

                    EntityNotFoundFargoApplicationException.ThrowIfNull(fromArticle, command.FromArticle.Value, EntityType.Article);

                    actor.ThrowIfAccessDenied(fromArticle);

                    article = Article.NewArticleVariation(command.Name, fromArticle);

                    break;
                }

            case ArticleType.Pack:
                {
                    var fromArticle = await articleRepository.GetByGuidAsync(command.FromArticle!.Value, cancellationToken);

                    EntityNotFoundFargoApplicationException.ThrowIfNull(fromArticle, command.FromArticle.Value, EntityType.Article);

                    actor.ThrowIfAccessDenied(fromArticle);

                    article = Article.NewArticlePack(command.Name, fromArticle, command.PackQuantity!.Value);

                    break;
                }

            case ArticleType.Kit:
                {
                    var kitComponents = new List<(Article, Scalar)>();

                    foreach (var kdo in command.KitComponents!)
                    {
                        var fromArticle = await articleRepository.GetByGuidAsync(kdo.ArticleGuid, cancellationToken);

                        EntityNotFoundFargoApplicationException.ThrowIfNull(fromArticle, kdo.ArticleGuid, EntityType.Article);

                        actor.ThrowIfAccessDenied(fromArticle);

                        var kit = (fromArticle, kdo.Quantity);

                        kitComponents.Add(kit);
                    }

                    article = Article.NewArticleKit(command.Name, kitComponents);

                    break;
                }

            case ArticleType.Container: article = Article.NewArticleContainer(command.Name); break;

            default: throw new NotSupportedException("Not supported article type.");
        }

        article.Description = command.Description ?? Description.Empty;

        article.ShelfLife = command.ShelfLife ?? null;

        article.Color = command.Color ?? null;

        article.SetMetrics(
            command.Mass ?? null,

            command.LengthX ?? null,

            command.LengthY ?? null,

            command.LengthZ ?? null);

        if (command.Ean13 is { } ean13)
        {
            await articleService.AssertArticleEan13IsAvailableAsync(ean13, cancellationToken);

            article.Barcode.Ean13 = ean13;
        }

        if (command.Ean8 is { } ean8)
        {
            await articleService.AssertArticleEan8IsAvailableAsync(ean8, cancellationToken);

            article.Barcode.Ean8 = ean8;
        }

        if (command.UpcA is { } upcA)
        {
            await articleService.AssertArticleUpcAIsAvailableAsync(upcA, cancellationToken);

            article.Barcode.UpcA = upcA;
        }

        if (command.UpcE is { } upcE)
        {
            await articleService.AssertArticleUpcEIsAvailableAsync(upcE, cancellationToken);

            article.Barcode.UpcE = upcE;
        }

        if (command.Code128 is { } code128)
        {
            await articleService.AssertArticleCode128IsAvailableAsync(code128, cancellationToken);

            article.Barcode.Code128 = code128;
        }

        if (command.Code39 is { } code39)
        {
            await articleService.AssertArticleCode39IsAvailableAsync(code39, cancellationToken);

            article.Barcode.Code39 = code39;
        }

        if (command.Itf14 is { } itf14)
        {
            await articleService.AssertArticleItf14IsAvailableAsync(itf14, cancellationToken);

            article.Barcode.Itf14 = itf14;
        }

        if (command.Gs1128 is { } gs1128)
        {
            await articleService.AssertArticleGs1128IsAvailableAsync(gs1128, cancellationToken);

            article.Barcode.Gs1128 = gs1128;
        }

        if (command.QrCode is { } qrCode)
        {
            await articleService.AssertArticleQrCodeIsAvailableAsync(qrCode, cancellationToken);

            article.Barcode.QrCode = qrCode;
        }

        if (command.DataMatrix is { } dataMatrix)
        {
            await articleService.AssertArticleDataMatrixIsAvailableAsync(dataMatrix, cancellationToken);

            article.Barcode.DataMatrix = dataMatrix;
        }

        if (command.PartitionsToAdd is { Count: > 0 } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                article.AddPartition(partition);
            }
        }

        articleRepository.Add(article);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CreateCompleted(article.Guid, actor.ActorId);

        return article.Guid;
    }
}
