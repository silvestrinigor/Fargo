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
    ILogger<ArticleCreateCommandHandler> logger) : ICommandHandler<ArticleCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(
        ArticleCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.CreateStarted(currentActor.ActorId);

        var dto = command.Article;

        var actor = await actorService.GetActorByActorIdAsync(currentActor.ActorId, cancellationToken);

        ActorAssertFound.ThrowNotAuthorizedIfNull(actor);

        actor.ThrowIfPermissionNotAuthorized(ActionType.CreateArticle);

        Article article;

        switch (dto.ArticleType)
        {
            case null:

            case ArticleType.Default:
                article = Article.NewArticle(dto.Name);
                break;

            case ArticleType.Variation:
                {
                    if (dto.FromArticle is null)
                    {
                        throw new InvalidOperationException();
                    }

                    var fromArticle = await articleRepository.GetByGuidAsync(dto.FromArticle.Value, cancellationToken);

                    EntityAssertFound.ThrowNotFoundIfNull(fromArticle);

                    article = Article.NewArticleVariation(dto.Name, fromArticle);

                    break;
                }

            case ArticleType.Pack:
                {
                    if (dto.FromArticle is null)
                    {
                        throw new InvalidOperationException();
                    }

                    if (dto.PackQuantity is null)
                    {
                        throw new InvalidOperationException();
                    }

                    var fromArticle = await articleRepository.GetByGuidAsync(dto.FromArticle.Value, cancellationToken);

                    EntityAssertFound.ThrowNotFoundIfNull(fromArticle);

                    article = Article.NewArticlePack(dto.Name, fromArticle, dto.PackQuantity.Value);

                    break;
                }

            case ArticleType.Kit: throw new NotImplementedException();

            case ArticleType.Container: article = Article.NewArticleContainer(dto.Name); break;

            default: throw new InvalidOperationException();
        }

        article.Description = dto.Description ?? Description.Empty;

        article.ShelfLife = dto.ShelfLife ?? null;

        article.Color = dto.Color ?? null;

        article.SetMetrics(
            dto.Mass ?? null,
            dto.LengthX ?? null,
            dto.LengthY ?? null,
            dto.LengthZ ?? null
        );

        if (dto.Ean13 is { } ean13)
        {
            await articleService.AssertArticleEan13IsAvailable(ean13, cancellationToken);

            article.Ean13 = ean13;
        }

        if (dto.Ean8 is { } ean8)
        {
            await articleService.AssertArticleEan8IsAvailable(ean8, cancellationToken);

            article.Ean8 = ean8;
        }

        if (dto.UpcA is { } upcA)
        {
            await articleService.AssertArticleUpcAIsAvailable(upcA, cancellationToken);

            article.UpcA = upcA;
        }

        if (dto.UpcE is { } upcE)
        {
            await articleService.AssertArticleUpcEIsAvailable(upcE, cancellationToken);

            article.UpcE = upcE;
        }

        if (dto.Code128 is { } code128)
        {
            await articleService.AssertArticleCode128IsAvailable(code128, cancellationToken);

            article.Code128 = code128;
        }

        if (dto.Code39 is { } code39)
        {
            await articleService.AssertArticleCode39IsAvailable(code39, cancellationToken);

            article.Code39 = code39;
        }

        if (dto.Itf14 is { } itf14)
        {
            await articleService.AssertArticleItf14IsAvailable(itf14, cancellationToken);

            article.Itf14 = itf14;
        }

        if (dto.Gs1128 is { } gs1128)
        {
            await articleService.AssertArticleGs1128IsAvailable(gs1128, cancellationToken);

            article.Gs1128 = gs1128;
        }

        if (dto.QrCode is { } qrCode)
        {
            await articleService.AssertArticleQrCodeIsAvailable(qrCode, cancellationToken);

            article.QrCode = qrCode;
        }

        if (dto.DataMatrix is { } dataMatrix)
        {
            await articleService.AssertArticleDataMatrixIsAvailable(dataMatrix, cancellationToken);

            article.DataMatrix = dataMatrix;
        }

        if (dto.Partitions is { Count: > 0 } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuid(partitionGuid, cancellationToken);

                EntityAssertFound.ThrowNotFoundIfNull(partition);

                actor.ThrowIfAccessNotAuthorized(partition);

                article.AddPartition(partition);
            }
        }

        article.IsActive = dto.IsActive ?? true;

        articleRepository.Add(article);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CreateCompleted(article.Guid, actor.ActorId);

        return article.Guid;
    }
}

