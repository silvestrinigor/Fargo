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
        logger.CreateStarted(currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.CreateArticle);

        Article article;

        switch (command.Create.ArticleType ?? ArticleType.Default)
        {
            case ArticleType.Default:
                article = Article.NewArticle(command.Create.Name);
                break;

            case ArticleType.Variation:
                {
                    var fromArticle = await articleRepository.GetByGuidAsync(command.Create.Variation!.FromArticleGuid, cancellationToken);

                    EntityNotFoundFargoApplicationException.ThrowIfNull(fromArticle, command.Create.Variation.FromArticleGuid, EntityType.Article);

                    actor.ThrowIfAccessDenied(fromArticle);

                    article = Article.NewArticleVariation(command.Create.Name, fromArticle);

                    break;
                }

            case ArticleType.Pack:
                {
                    var fromArticle = await articleRepository.GetByGuidAsync(command.Create.Pack!.FromArticleGuid, cancellationToken);

                    EntityNotFoundFargoApplicationException.ThrowIfNull(fromArticle, command.Create.Pack.FromArticleGuid, EntityType.Article);

                    actor.ThrowIfAccessDenied(fromArticle);

                    article = Article.NewArticlePack(command.Create.Name, fromArticle, command.Create.Pack.Quantity);

                    break;
                }

            case ArticleType.Kit:
                {
                    var kitComponents = new List<(Article, Scalar)>();

                    foreach (var kdo in command.Create.KitComponents!)
                    {
                        var fromArticle = await articleRepository.GetByGuidAsync(kdo.ArticleGuid, cancellationToken);

                        EntityNotFoundFargoApplicationException.ThrowIfNull(fromArticle, kdo.ArticleGuid, EntityType.Article);

                        actor.ThrowIfAccessDenied(fromArticle);

                        var kit = (fromArticle, kdo.Quantity);

                        kitComponents.Add(kit);
                    }

                    article = Article.NewArticleKit(command.Create.Name, kitComponents);

                    break;
                }

            case ArticleType.Container: article = Article.NewArticleContainer(command.Create.Name); break;

            default: throw new NotSupportedException("Not supported article type.");
        }

        article.Description = command.Create.Description ?? Description.Empty;

        article.ShelfLife = command.Create.ShelfLife ?? null;

        article.Color = command.Create.Color ?? null;

        article.SetMetrics(
            command.Create.Mass ?? null,

            command.Create.Dimension?.LengthX ?? null,

            command.Create.Dimension?.LengthY ?? null,

            command.Create.Dimension?.LengthZ ?? null);

        if (command.Create.Barcode?.Ean13 is { } ean13)
        {
            await articleService.AssertArticleEan13IsAvailableAsync(ean13, cancellationToken);

            article.Barcode.Ean13 = ean13;
        }

        if (command.Create.Barcode?.Ean8 is { } ean8)
        {
            await articleService.AssertArticleEan8IsAvailableAsync(ean8, cancellationToken);

            article.Barcode.Ean8 = ean8;
        }

        if (command.Create.Barcode?.UpcA is { } upcA)
        {
            await articleService.AssertArticleUpcAIsAvailableAsync(upcA, cancellationToken);

            article.Barcode.UpcA = upcA;
        }

        if (command.Create.Barcode?.UpcE is { } upcE)
        {
            await articleService.AssertArticleUpcEIsAvailableAsync(upcE, cancellationToken);

            article.Barcode.UpcE = upcE;
        }

        if (command.Create.Barcode?.Code128 is { } code128)
        {
            await articleService.AssertArticleCode128IsAvailableAsync(code128, cancellationToken);

            article.Barcode.Code128 = code128;
        }

        if (command.Create.Barcode?.Code39 is { } code39)
        {
            await articleService.AssertArticleCode39IsAvailableAsync(code39, cancellationToken);

            article.Barcode.Code39 = code39;
        }

        if (command.Create.Barcode?.Itf14 is { } itf14)
        {
            await articleService.AssertArticleItf14IsAvailableAsync(itf14, cancellationToken);

            article.Barcode.Itf14 = itf14;
        }

        if (command.Create.Barcode?.Gs1128 is { } gs1128)
        {
            await articleService.AssertArticleGs1128IsAvailableAsync(gs1128, cancellationToken);

            article.Barcode.Gs1128 = gs1128;
        }

        if (command.Create.Barcode?.QrCode is { } qrCode)
        {
            await articleService.AssertArticleQrCodeIsAvailableAsync(qrCode, cancellationToken);

            article.Barcode.QrCode = qrCode;
        }

        if (command.Create.Barcode?.DataMatrix is { } dataMatrix)
        {
            await articleService.AssertArticleDataMatrixIsAvailableAsync(dataMatrix, cancellationToken);

            article.Barcode.DataMatrix = dataMatrix;
        }

        if (command.Create.PartitionsToAdd is { Count: > 0 } partitionsToAdd)
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

        logger.CreateCompleted(article.Guid, actor.Guid);

        return article.Guid;
    }
}
