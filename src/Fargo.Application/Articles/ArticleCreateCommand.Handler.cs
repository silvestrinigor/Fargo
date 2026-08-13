using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Audits;
using Fargo.Core.Entities;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Fargo.Application.Articles;

public sealed class ArticleCreateCommandHandler(
    ArticleService articleService, ActorResolver actorService,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
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

        AuditLog articleAudit;

        switch (command.Create.ArticleType ?? ArticleType.Default)
        {
            case ArticleType.Default:
                {
                    article = Article.NewArticle(command.Create.Name);

                    articleAudit = AuditLog.CreateAuditLog(actor, article, ActionType.CreateArticle);

                    break;
                }

            case ArticleType.Variation:
                {
                    var fromArticle = await articleRepository.GetByGuidAsync(command.Create.Variation!.FromArticleGuid, cancellationToken);

                    EntityNotFoundFargoApplicationException.ThrowIfNull(fromArticle, command.Create.Variation.FromArticleGuid, EntityType.Article);

                    actor.ThrowIfAccessDenied(fromArticle);

                    article = Article.NewArticleVariation(command.Create.Name, fromArticle);

                    articleAudit = AuditLog.CreateAuditLog(actor, article, ActionType.CreateArticle);

                    var auditVariation = new Dictionary<string, AuditValue>
                    {
                        { AuditPropertyNames.ArticleCreated.ArticleVariationFromArticleGuid, new AuditValue.String(fromArticle.Guid.ToString()) }
                    };

                    articleAudit.Metadata.Add(AuditPropertyNames.ArticleCreated.ArticleVariation, new AuditValue.Object(auditVariation));

                    break;
                }

            case ArticleType.Pack:
                {
                    var fromArticle = await articleRepository.GetByGuidAsync(command.Create.Pack!.FromArticleGuid, cancellationToken);

                    EntityNotFoundFargoApplicationException.ThrowIfNull(fromArticle, command.Create.Pack.FromArticleGuid, EntityType.Article);

                    actor.ThrowIfAccessDenied(fromArticle);

                    article = Article.NewArticlePack(command.Create.Name, fromArticle, command.Create.Pack.Quantity);

                    articleAudit = AuditLog.CreateAuditLog(actor, article, ActionType.CreateArticle);

                    var auditVariation = new Dictionary<string, AuditValue>
                    {
                        { AuditPropertyNames.ArticleCreated.ArticlePackFromArticleGuid, new AuditValue.String(fromArticle.Guid.ToString()) },
                        { AuditPropertyNames.ArticleCreated.ArticlePackQuantity, new AuditValue.String(article.Pack!.Quantity.ToString()) }
                    };

                    articleAudit.Metadata.Add(AuditPropertyNames.ArticleCreated.ArticlePack, new AuditValue.Object(auditVariation));

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

                    articleAudit = AuditLog.CreateAuditLog(actor, article, ActionType.CreateArticle);

                    break;
                }

            case ArticleType.Container:
                {
                    article = Article.NewArticleContainer(command.Create.Name);

                    articleAudit = AuditLog.CreateAuditLog(actor, article, ActionType.CreateArticle);

                    break;
                }

            default: throw new NotSupportedException("Article type not supported.");
        }

        articleAudit.Metadata.Add(AuditPropertyNames.ArticleCreated.ArticleName, new AuditValue.String(article.Name));

        articleAudit.Metadata.Add(AuditPropertyNames.ArticleCreated.ArticleType, new AuditValue.Number((byte)article.ArticleType));

        article.Description = command.Create.Description ?? Description.Empty;

        if (command.Create.ShelfLife is { } shelfLife)
        {
            article.ShelfLife = shelfLife;
        }

        article.Color = command.Create.Color ?? null;

        article.SetMetrics(
            command.Create.Mass ?? null,

            command.Create.Dimension?.LengthX ?? null,

            command.Create.Dimension?.LengthY ?? null,

            command.Create.Dimension?.LengthZ ?? null);

        if (command.Create.Barcode?.Ean13 is { } ean13)
        {
            await articleService.ValidateEan13IsAvailableAsync(ean13, cancellationToken);

            article.Barcode.Ean13 = ean13;
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

        auditLogRepository.Add(articleAudit);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CreateCompleted(article.Guid, actor.Guid);

        return article.Guid;
    }
}
