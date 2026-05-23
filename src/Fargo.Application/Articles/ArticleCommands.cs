using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

#region Create

/// <summary>
/// Command used to create a new article from an API creation payload.
/// </summary>
public sealed record ArticleCreateCommand(
    ArticleCreateDto Create
) : ICommand<Guid>;

/// <summary>
/// Handles article creation, including optional create-time state.
/// </summary>
public sealed class ArticleCreateCommandHandler(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    IEntityPartitionEventRepository entityPartitionEventRepository,
    ArticleService articleService,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<ArticleCreateCommandHandler> logger
) : ICommandHandler<ArticleCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();
        var create = command.Create;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Article create flow started for actor {ActorGuid}.", actor.Guid);
        }

        var createTypeCount =
            (create.Variation is null ? 0 : 1) +
            (create.Pack is null ? 0 : 1) +
            (create.Kit is null ? 0 : 1) +
            (create.Container is null ? 0 : 1);

        if (createTypeCount > 1)
        {
            throw new ArgumentException(
                "Article creation accepts only one specialized article type.",
                nameof(command));
        }

        Article article;

        if (create.Variation is { } variation)
        {
            var fromArticle = await articleRepository.GetFoundByGuid(
                variation.FromArticleGuid,
                cancellationToken);

            article = Article.CreateArticleVariation(create.Name, fromArticle, actor);
        }
        else if (create.Pack is { } pack)
        {
            var fromArticle = await articleRepository.GetFoundByGuid(
                pack.FromArticleGuid,
                cancellationToken);

            article = Article.CreateArticlePack(create.Name, fromArticle, pack.Quantity, actor);
        }
        else if (create.Kit is { } kit)
        {
            if (kit.Packs.Count == 0)
            {
                throw new ArgumentException(
                    "Kit article creation requires at least one pack.",
                    nameof(create));
            }

            var components = new List<ArticleKitComponent>(kit.Packs.Count);

            foreach (var componentPack in kit.Packs)
            {
                var fromArticle = await articleRepository.GetFoundByGuid(
                    componentPack.ArticleGuid,
                    cancellationToken);

                components.Add(new ArticleKitComponent(fromArticle, componentPack.Quantity));
            }

            article = Article.CreateArticleKit(create.Name, components, actor);
        }
        else if (create.Container is not null)
        {
            article = Article.CreateArticleContainer(create.Name, actor);
        }
        else
        {
            article = Article.CreateArticle(create.Name, actor);
        }

        articleRepository.Add(article);

        entityEventRepository.Add(EntityEvent.EntityCreated<Article>(article, actor.Guid));

        if (create.Description is { } description)
        {
            article.ChangeDescription(description, actor);
        }

        if (create.ShelfLife is not null)
        {
            article.SetShelfLife(create.ShelfLife, actor);
        }

        if (create.Color is not null)
        {
            article.SetColor(create.Color, actor);
        }

        if (create.Metrics is { } metrics)
        {
            article.SetMetrics(metrics.ToCore(), actor);
        }

        if (create.Barcodes is { } barcodes)
        {
            var articleBarcodes = barcodes.ToCore();

            await articleService.SetEan13(articleBarcodes.Ean13, article, actor, cancellationToken);
            await articleService.SetEan8(articleBarcodes.Ean8, article, actor, cancellationToken);
            await articleService.SetUpcA(articleBarcodes.UpcA, article, actor, cancellationToken);
            await articleService.SetUpcE(articleBarcodes.UpcE, article, actor, cancellationToken);
            await articleService.SetCode128(articleBarcodes.Code128, article, actor, cancellationToken);
            await articleService.SetCode39(articleBarcodes.Code39, article, actor, cancellationToken);
            await articleService.SetItf14(articleBarcodes.Itf14, article, actor, cancellationToken);
            await articleService.SetGs1128(articleBarcodes.Gs1128, article, actor, cancellationToken);
            await articleService.SetQrCode(articleBarcodes.QrCode, article, actor, cancellationToken);
            await articleService.SetDataMatrix(articleBarcodes.DataMatrix, article, actor, cancellationToken);
        }

        if (create.Container?.MaxMass is { } containerMaxMass)
        {
            article.SetContainerMaxMass(containerMaxMass, actor);
        }

        if (create.Partitions is { Count: > 0 } partitions)
        {
            foreach (var partitionGuid in partitions.Distinct())
            {
                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                article.AddPartition(partition, actor);

                entityPartitionEventRepository.Add(EntityPartitionEvent.InsertedIntoPartition(
                    article,
                    partition,
                    actor.Guid));
            }
        }

        if (create.IsActive == false)
        {
            article.Deactivate(actor);

            entityEventRepository.Add(EntityEvent.Deactivated<Article>(article, actor.Guid));
        }

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article create mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }

        return article.Guid;
    }
}

#endregion Create

#region Patch

/// <summary>
/// Command used to patch an existing article from an API patch payload.
/// </summary>
public sealed record ArticlePatchCommand(
    Guid ArticleGuid,
    ArticlePatchDto Patch
) : ICommand;

/// <summary>
/// Handles partial article updates.
/// </summary>
public sealed class ArticlePatchCommandHandler(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    IEntityPartitionEventRepository entityPartitionEventRepository,
    ArticleService articleService,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<ArticlePatchCommandHandler> logger
) : ICommandHandler<ArticlePatchCommand>
{
    public async Task Handle(
        ArticlePatchCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();
        var patch = command.Patch;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article patch flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.Guid);
        }

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        article.ValidateCanEdit(actor);

        if (patch.Name is { } name)
        {
            article.Rename(name, actor);
        }

        if (patch.Description is { } description)
        {
            article.ChangeDescription(description, actor);
        }

        if (patch.ShelfLife.IsSpecified)
        {
            article.SetShelfLife(patch.ShelfLife.Value, actor);
        }

        if (patch.Metrics is { } metrics)
        {
            article.SetMetrics(metrics.ToCore(), actor);
        }

        if (patch.Barcodes is { } barcodes)
        {
            var articleBarcodes = barcodes.ToCore();

            await articleService.SetEan13(articleBarcodes.Ean13, article, actor, cancellationToken);
            await articleService.SetEan8(articleBarcodes.Ean8, article, actor, cancellationToken);
            await articleService.SetUpcA(articleBarcodes.UpcA, article, actor, cancellationToken);
            await articleService.SetUpcE(articleBarcodes.UpcE, article, actor, cancellationToken);
            await articleService.SetCode128(articleBarcodes.Code128, article, actor, cancellationToken);
            await articleService.SetCode39(articleBarcodes.Code39, article, actor, cancellationToken);
            await articleService.SetItf14(articleBarcodes.Itf14, article, actor, cancellationToken);
            await articleService.SetGs1128(articleBarcodes.Gs1128, article, actor, cancellationToken);
            await articleService.SetQrCode(articleBarcodes.QrCode, article, actor, cancellationToken);
            await articleService.SetDataMatrix(articleBarcodes.DataMatrix, article, actor, cancellationToken);
        }

        if (patch.Partitions is { } partitions)
        {
            var requestedPartitionGuids = partitions.Distinct().ToArray();

            foreach (var partitionGuid in requestedPartitionGuids)
            {
                if (article.Partitions.Any(p => p.Guid == partitionGuid))
                {
                    continue;
                }

                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                article.AddPartition(partition, actor);

                entityPartitionEventRepository.Add(EntityPartitionEvent.InsertedIntoPartition(
                    article,
                    partition,
                    actor.Guid));
            }

            var partitionsToRemove = article.Partitions
                .Where(p => !requestedPartitionGuids.Contains(p.Guid))
                .ToList();

            foreach (var partition in partitionsToRemove)
            {
                article.RemovePartition(partition, actor);

                entityPartitionEventRepository.Add(EntityPartitionEvent.RemovedFromPartition(
                    article,
                    partition,
                    actor.Guid));
            }
        }

        if (patch.IsActive is { } isActive)
        {
            if (isActive && !article.IsActive)
            {
                article.Activate(actor);
                entityEventRepository.Add(EntityEvent.Activated<Article>(article, actor.Guid));
            }
            else if (!isActive && article.IsActive)
            {
                article.Deactivate(actor);
                entityEventRepository.Add(EntityEvent.Deactivated<Article>(article, actor.Guid));
            }
        }

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article patch mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }
    }
}

#endregion Patch

#region Delete

/// <summary>
/// Command used to delete an article.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
public sealed record ArticleDeleteCommand(
    Guid ArticleGuid
) : ICommand;

/// <summary>
/// Handles article deletion.
/// </summary>
public sealed class ArticleDeleteCommandHandler(
    IArticleRepository articleRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<ArticleDeleteCommandHandler> logger
) : ICommandHandler<ArticleDeleteCommand>
{
    public async Task Handle(
        ArticleDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article delete flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.Guid);
        }

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        article.ValidateCanDelete(actor);

        var hasItems = await articleRepository.HasItemsAssociated(
            article.Guid,
            cancellationToken);

        if (hasItems)
        {
            logger.LogWarning(
                "Article delete flow rejected because article {ArticleGuid} has associated items.",
                article.Guid);
            throw new ArticleDeleteWithItemsAssociatedFargoDomainException(article.Guid);
        }

        articleRepository.Remove(article);

        entityEventRepository.Add(EntityEvent.EntityDeleted<Article>(article, actor.Guid));

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article delete mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }
    }
}

#endregion Delete
