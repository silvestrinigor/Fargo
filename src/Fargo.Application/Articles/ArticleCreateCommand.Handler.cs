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

/// <summary>
/// Handles commands that create articles.
/// </summary>
/// <param name="articleService">
/// Provides article-specific operations and validation.
/// </param>
/// <param name="actorService">
/// Resolves the current actor and its permissions and partition access.
/// </param>
/// <param name="articleRepository">
/// Provides access to article entities.
/// </param>
/// <param name="partitionRepository">
/// Provides access to partitions associated with the article.
/// </param>
/// <param name="auditLogRepository">
/// Persists the audit log generated for the article creation.
/// </param>
/// <param name="currentActor">
/// Provides information about the currently authenticated actor.
/// </param>
/// <param name="unitOfWork">
/// Coordinates persistence of the article and its audit log.
/// </param>
/// <param name="logger">
/// Logs the execution of the command.
/// </param>
public sealed class ArticleCreateCommandHandler(
    ArticleService articleService, ActorResolver actorService,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IAuditLogRepository auditLogRepository,
    ICurrentActor currentActor, IUnitOfWork unitOfWork,
    ILogger<ArticleCreateCommandHandler> logger
    ) : ICommandHandler<ArticleCreateCommand, Guid>
{
    /// <summary>
    /// Creates an article according to the requested article type, validates the
    /// current actor's permissions and access to referenced entities, associates
    /// the article with the requested partitions, and records the operation in
    /// the audit log.
    /// </summary>
    /// <param name="command">
    /// The command containing the article creation data.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// The unique identifier of the newly created article.
    /// </returns>
    /// <exception cref="ActorNotFoundFargoApplicationException">
    /// Thrown when the current actor cannot be found.
    /// </exception>
    /// <exception cref="FargoApplicationException">
    /// Thrown when required data for the requested article type is missing.
    /// </exception>
    /// <exception cref="EntityNotFoundFargoApplicationException">
    /// Thrown when a referenced article or partition cannot be found.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Thrown when the requested article type is not supported.
    /// </exception>
    public async Task<Guid> HandleAsync(
        ArticleCreateCommand command, CancellationToken cancellationToken = default)
    {
        logger.CreateStarted(currentActor.Guid, currentActor.ActorType);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.CreateArticle);

        Article article;

        AuditLog articleAudit;

        switch (command.Create.ArticleType)
        {
            case ArticleType.Default:
                {
                    article = Article.NewArticle(command.Create.Name);

                    articleAudit = AuditLog.CreateAuditLog(actor, article, ActionType.CreateArticle);

                    break;
                }

            case ArticleType.Variation:
                {
                    if (command.Create.Variation?.FromArticleGuid is null)
                    {
                        throw new FargoApplicationException(
                            "Variation from article guid must be informed when the article type is variation.");
                    }

                    var fromArticle = await articleRepository.GetByGuidAsync(command.Create.Variation.FromArticleGuid, cancellationToken);

                    EntityNotFoundFargoApplicationException.ThrowIfNull(fromArticle, command.Create.Variation.FromArticleGuid, EntityType.Article);

                    actor.ThrowIfAccessDenied(fromArticle);

                    article = Article.NewArticleVariation(command.Create.Name, fromArticle);

                    articleAudit = AuditLog.CreateAuditLog(actor, article, ActionType.CreateArticle);

                    break;
                }

            case ArticleType.Pack:
                {
                    if (command.Create.Pack?.FromArticleGuid is null)
                    {
                        throw new FargoApplicationException(
                            "Pack from article guid must be informed when the article type is pack.");
                    }

                    if (command.Create.Pack?.Quantity is null)
                    {
                        throw new FargoApplicationException(
                            "Pack quantity should be informed when article type is pack.");
                    }

                    var fromArticle = await articleRepository.GetByGuidAsync(command.Create.Pack!.FromArticleGuid, cancellationToken);

                    EntityNotFoundFargoApplicationException.ThrowIfNull(fromArticle, command.Create.Pack.FromArticleGuid, EntityType.Article);

                    actor.ThrowIfAccessDenied(fromArticle);

                    article = Article.NewArticlePack(command.Create.Name, fromArticle, command.Create.Pack.Quantity);

                    articleAudit = AuditLog.CreateAuditLog(actor, article, ActionType.CreateArticle);

                    break;
                }

            case ArticleType.Kit:
                {
                    if (command.Create.KitComponents is null || command.Create.KitComponents.Count == 0)
                    {
                        throw new FargoApplicationException(
                            "Kit components should be informed when article type is kit.");
                    }

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

        articleAudit.Metadata.AddName(article.Name);

        articleAudit.Metadata.AddArticleType(article.ArticleType);

        article.Description = command.Create.Description ?? Description.Empty;

        articleAudit.Metadata.AddDescription(article.Description);

        if (command.Create.ShelfLife is { } shelfLife)
        {
            article.ShelfLife = shelfLife;
        }

        article.Color = command.Create.Color ?? null;

        article.SetMetrics(
            command.Create.Mass ?? null,

            command.Create.Dimension?.LengthX ?? null,

            command.Create.Dimension?.LengthY ?? null,

            command.Create.Dimension?.LengthZ ?? null
        );

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

        logger.CreateCompleted(article.Guid, actor.Guid, actor.ActorType);

        return article.Guid;
    }
}
