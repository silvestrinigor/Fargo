using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Articles;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

public sealed class ArticleCreateCommandHandler(
    ArticleService articleService, ActorService actorService,
    IArticleRepository articleRepository, IPartitionRepository partitionRepository,
    ICurrentActor currentActor, IUnitOfWork unitOfWork,
    ILogger<ArticleCreateCommandHandler> logger
    ) : ICommandHandler<ArticleCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        ArticleCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.CreateStarted(currentActor.ActorId);

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.CreateArticle);

        Article article;

        switch (command.ArticleType)
        {
            case ArticleType.Default:
                article = Article.NewArticle(command.Name);
                break;

            case ArticleType.Variation:
                {
                    var fromArticle = await articleRepository.GetByGuidAsync(command.FromArticle!.Value, cancellationToken);

                    EntityAssertFound.ThrowNotFoundIfNull(fromArticle);

                    actor.ThrowIfAccessNotAuthorized(fromArticle);

                    article = Article.NewArticleVariation(command.Name, fromArticle);

                    break;
                }

            case ArticleType.Pack:
                {
                    var fromArticle = await articleRepository.GetByGuidAsync(command.FromArticle!.Value, cancellationToken);

                    EntityAssertFound.ThrowNotFoundIfNull(fromArticle);

                    actor.ThrowIfAccessNotAuthorized(fromArticle);

                    article = Article.NewArticlePack(command.Name, fromArticle, command.PackQuantity!.Value);

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

            article.Ean13 = ean13;
        }

        if (command.Ean8 is { } ean8)
        {
            await articleService.AssertArticleEan8IsAvailableAsync(ean8, cancellationToken);

            article.Ean8 = ean8;
        }

        if (command.UpcA is { } upcA)
        {
            await articleService.AssertArticleUpcAIsAvailableAsync(upcA, cancellationToken);

            article.UpcA = upcA;
        }

        if (command.UpcE is { } upcE)
        {
            await articleService.AssertArticleUpcEIsAvailableAsync(upcE, cancellationToken);

            article.UpcE = upcE;
        }

        if (command.Code128 is { } code128)
        {
            await articleService.AssertArticleCode128IsAvailableAsync(code128, cancellationToken);

            article.Code128 = code128;
        }

        if (command.Code39 is { } code39)
        {
            await articleService.AssertArticleCode39IsAvailableAsync(code39, cancellationToken);

            article.Code39 = code39;
        }

        if (command.Itf14 is { } itf14)
        {
            await articleService.AssertArticleItf14IsAvailableAsync(itf14, cancellationToken);

            article.Itf14 = itf14;
        }

        if (command.Gs1128 is { } gs1128)
        {
            await articleService.AssertArticleGs1128IsAvailableAsync(gs1128, cancellationToken);

            article.Gs1128 = gs1128;
        }

        if (command.QrCode is { } qrCode)
        {
            await articleService.AssertArticleQrCodeIsAvailableAsync(qrCode, cancellationToken);

            article.QrCode = qrCode;
        }

        if (command.DataMatrix is { } dataMatrix)
        {
            await articleService.AssertArticleDataMatrixIsAvailableAsync(dataMatrix, cancellationToken);

            article.DataMatrix = dataMatrix;
        }

        if (command.PartitionsToAdd is { Count: > 0 } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityAssertFound.ThrowNotFoundIfNull(partition);

                actor.ThrowIfAccessNotAuthorized(partition);

                article.AddPartition(partition);
            }
        }

        article.IsActive = command.IsActive ?? true;

        articleRepository.Add(article);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CreateCompleted(article.Guid, actor.ActorId);

        return article.Guid;
    }
}
