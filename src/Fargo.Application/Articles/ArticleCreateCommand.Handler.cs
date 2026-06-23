using Fargo.Application.Actors;
using Fargo.Application.Entities;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Articles;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

public sealed class ArticleCreateCommandHandler(
    ArticleService articleService, ActorQueryService actorService,
    IArticleRepository articleRepository, IPartitionRepository partitionRepository,
    ICurrentActor currentActor, IUnitOfWork unitOfWork,
    ILogger<ArticleCreateCommandHandler> logger) : ICommandHandler<ArticleCreateCommand, Guid>
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
            case null:

            case ArticleType.Default: article = Article.NewArticle(command.Name); break;

            case ArticleType.Variation:
                {
                    if (command.FromArticle is null)
                    {
                        throw new InvalidOperationException();
                    }

                    var fromArticle = await articleRepository.GetByGuidAsync(command.FromArticle.Value, cancellationToken);

                    EntityAssertFound.ThrowNotFoundIfNull(fromArticle);

                    article = Article.NewArticleVariation(command.Name, fromArticle);

                    break;
                }

            case ArticleType.Pack:
                {
                    if (command.FromArticle is null)
                    {
                        throw new InvalidOperationException();
                    }

                    if (command.PackQuantity is null)
                    {
                        throw new InvalidOperationException();
                    }

                    var fromArticle = await articleRepository.GetByGuidAsync(command.FromArticle.Value, cancellationToken);

                    EntityAssertFound.ThrowNotFoundIfNull(fromArticle);

                    article = Article.NewArticlePack(command.Name, fromArticle, command.PackQuantity.Value);

                    break;
                }

            case ArticleType.Kit: throw new NotImplementedException();

            case ArticleType.Container: article = Article.NewArticleContainer(command.Name); break;

            default: throw new InvalidOperationException();
        }

        article.Description = command.Description ?? Description.Empty;

        article.ShelfLife = command.ShelfLife ?? null;

        article.Color = command.Color ?? null;

        article.SetMetrics(
            command.Mass ?? null,
            command.LengthX ?? null,
            command.LengthY ?? null,
            command.LengthZ ?? null
        );

        if (command.Ean13 is { } ean13)
        {
            await articleService.AssertArticleEan13IsAvailable(ean13, cancellationToken);

            article.Ean13 = ean13;
        }

        if (command.Ean8 is { } ean8)
        {
            await articleService.AssertArticleEan8IsAvailable(ean8, cancellationToken);

            article.Ean8 = ean8;
        }

        if (command.UpcA is { } upcA)
        {
            await articleService.AssertArticleUpcAIsAvailable(upcA, cancellationToken);

            article.UpcA = upcA;
        }

        if (command.UpcE is { } upcE)
        {
            await articleService.AssertArticleUpcEIsAvailable(upcE, cancellationToken);

            article.UpcE = upcE;
        }

        if (command.Code128 is { } code128)
        {
            await articleService.AssertArticleCode128IsAvailable(code128, cancellationToken);

            article.Code128 = code128;
        }

        if (command.Code39 is { } code39)
        {
            await articleService.AssertArticleCode39IsAvailable(code39, cancellationToken);

            article.Code39 = code39;
        }

        if (command.Itf14 is { } itf14)
        {
            await articleService.AssertArticleItf14IsAvailable(itf14, cancellationToken);

            article.Itf14 = itf14;
        }

        if (command.Gs1128 is { } gs1128)
        {
            await articleService.AssertArticleGs1128IsAvailable(gs1128, cancellationToken);

            article.Gs1128 = gs1128;
        }

        if (command.QrCode is { } qrCode)
        {
            await articleService.AssertArticleQrCodeIsAvailable(qrCode, cancellationToken);

            article.QrCode = qrCode;
        }

        if (command.DataMatrix is { } dataMatrix)
        {
            await articleService.AssertArticleDataMatrixIsAvailable(dataMatrix, cancellationToken);

            article.DataMatrix = dataMatrix;
        }

        if (command.Partitions is { Count: > 0 } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuid(partitionGuid, cancellationToken);

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

