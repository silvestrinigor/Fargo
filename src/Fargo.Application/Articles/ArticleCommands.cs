using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;
using System.Drawing;
using UnitsNet;

namespace Fargo.Application.Articles;

#region Create

/// <summary>
/// Command used to create a default article.
/// </summary>
public sealed record ArticleCreateDefaultCommand(
    Name Name,
    Description? Description = null,
    TimeSpan? ShelfLife = null,
    Color? Color = null,
    ArticleMetricsDto? Metrics = null,
    ArticleBarcodesDto? Barcodes = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    bool? IsActive = null
) : ICommand<Guid>;

/// <summary>
/// Handles default article creation.
/// </summary>
public sealed class ArticleCreateDefaultCommandHandler(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    IEntityPartitionEventRepository entityPartitionEventRepository,
    ArticleService articleService,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<ArticleCreateDefaultCommandHandler> logger
) : ICommandHandler<ArticleCreateDefaultCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateDefaultCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);

        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article create flow started for actor {ActorGuid}.",
                actor.Guid);
        }

        var article = Article.CreateArticle(command.Name, actor);

        articleRepository.Add(article);

        entityEventRepository.Add(EntityEvent.EntityCreated(article, actor.Guid));

        if (command.Description is { } description)
        {
            article.ChangeDescription(description, actor);
        }

        if (command.ShelfLife is not null)
        {
            article.SetShelfLife(command.ShelfLife, actor);
        }

        if (command.Color is not null)
        {
            article.SetColor(command.Color, actor);
        }

        if (command.Metrics is { } metrics)
        {
            article.SetMetrics(metrics.ToCore(), actor);
        }

        if (command.Barcodes is { } barcodes)
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

        if (command.Partitions is { Count: > 0 } partitions)
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

        if (command.IsActive == false)
        {
            article.Deactivate(actor);

            entityEventRepository.Add(EntityEvent.Deactivated(article, actor.Guid));
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

/// <summary>
/// Command used to create a variation article.
/// </summary>
public sealed record ArticleCreateVariationCommand(
    Name Name,
    Guid FromArticleGuid,
    Description? Description = null,
    TimeSpan? ShelfLife = null,
    Color? Color = null,
    ArticleMetricsDto? Metrics = null,
    ArticleBarcodesDto? Barcodes = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    bool? IsActive = null
) : ICommand<Guid>;

/// <summary>
/// Handles variation article creation.
/// </summary>
public sealed class ArticleCreateVariationCommandHandler(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    IEntityPartitionEventRepository entityPartitionEventRepository,
    ArticleService articleService,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<ArticleCreateVariationCommandHandler> logger
) : ICommandHandler<ArticleCreateVariationCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateVariationCommand command,
        CancellationToken cancellationToken = default)
    {
        const ArticleType expectedArticleType = ArticleType.Variation;

        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article {ArticleType} create flow started for actor {ActorGuid}.",
                expectedArticleType,
                actor.Guid);
        }

        var fromArticle = await articleRepository.GetFoundByGuid(
            command.FromArticleGuid,
            cancellationToken);

        var article = Article.CreateArticleVariation(command.Name, fromArticle, actor);

        articleRepository.Add(article);

        entityEventRepository.Add(EntityEvent.EntityCreated<Article>(article, actor.Guid));

        if (command.Description is { } description)
        {
            article.ChangeDescription(description, actor);
        }

        if (command.ShelfLife is not null)
        {
            article.SetShelfLife(command.ShelfLife, actor);
        }

        if (command.Color is not null)
        {
            article.SetColor(command.Color, actor);
        }

        if (command.Metrics is { } metrics)
        {
            article.SetMetrics(metrics.ToCore(), actor);
        }

        if (command.Barcodes is { } barcodes)
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

        if (command.Partitions is { Count: > 0 } partitions)
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

        if (command.IsActive == false)
        {
            article.Deactivate(actor);

            entityEventRepository.Add(EntityEvent.Deactivated<Article>(article, actor.Guid));
        }

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article {ArticleType} create mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                expectedArticleType,
                article.Guid,
                actor.Guid);
        }

        return article.Guid;
    }
}

/// <summary>
/// Command used to create a pack article.
/// </summary>
public sealed record ArticleCreatePackCommand(
    Name Name,
    Guid FromArticleGuid,
    Scalar Quantity,
    Description? Description = null,
    TimeSpan? ShelfLife = null,
    Color? Color = null,
    ArticleMetricsDto? Metrics = null,
    ArticleBarcodesDto? Barcodes = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    bool? IsActive = null
) : ICommand<Guid>;

/// <summary>
/// Handles pack article creation.
/// </summary>
public sealed class ArticleCreatePackCommandHandler(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    IEntityPartitionEventRepository entityPartitionEventRepository,
    ArticleService articleService,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<ArticleCreatePackCommandHandler> logger
) : ICommandHandler<ArticleCreatePackCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreatePackCommand command,
        CancellationToken cancellationToken = default)
    {
        const ArticleType expectedArticleType = ArticleType.Pack;

        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article {ArticleType} create flow started for actor {ActorGuid}.",
                expectedArticleType,
                actor.Guid);
        }

        var fromArticle = await articleRepository.GetFoundByGuid(
            command.FromArticleGuid,
            cancellationToken);

        var article = Article.CreateArticlePack(command.Name, fromArticle, command.Quantity, actor);

        articleRepository.Add(article);

        entityEventRepository.Add(EntityEvent.EntityCreated<Article>(article, actor.Guid));

        if (command.Description is { } description)
        {
            article.ChangeDescription(description, actor);
        }

        if (command.ShelfLife is not null)
        {
            article.SetShelfLife(command.ShelfLife, actor);
        }

        if (command.Color is not null)
        {
            article.SetColor(command.Color, actor);
        }

        if (command.Metrics is { } metrics)
        {
            article.SetMetrics(metrics.ToCore(), actor);
        }

        if (command.Barcodes is { } barcodes)
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

        if (command.Partitions is { Count: > 0 } partitions)
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

        if (command.IsActive == false)
        {
            article.Deactivate(actor);

            entityEventRepository.Add(EntityEvent.Deactivated<Article>(article, actor.Guid));
        }

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article {ArticleType} create mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                expectedArticleType,
                article.Guid,
                actor.Guid);
        }

        return article.Guid;
    }
}

/// <summary>
/// Command used to create a kit article.
/// </summary>
public sealed record ArticleCreateKitCommand(
    Name Name,
    IReadOnlyCollection<ArticleCreateKitPackDto> Packs,
    Description? Description = null,
    TimeSpan? ShelfLife = null,
    Color? Color = null,
    ArticleMetricsDto? Metrics = null,
    ArticleBarcodesDto? Barcodes = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    bool? IsActive = null
) : ICommand<Guid>;

/// <summary>
/// Handles kit article creation.
/// </summary>
public sealed class ArticleCreateKitCommandHandler(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    IEntityPartitionEventRepository entityPartitionEventRepository,
    ArticleService articleService,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<ArticleCreateKitCommandHandler> logger
) : ICommandHandler<ArticleCreateKitCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateKitCommand command,
        CancellationToken cancellationToken = default)
    {
        const ArticleType expectedArticleType = ArticleType.Kit;

        ArticleCreateRequestValidator.ValidateKitPacks(command.Packs);

        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article {ArticleType} create flow started for actor {ActorGuid}.",
                expectedArticleType,
                actor.Guid);
        }

        var components = new List<ArticleKitComponent>(command.Packs.Count);

        foreach (var componentPack in command.Packs)
        {
            var fromArticle = await articleRepository.GetFoundByGuid(
                componentPack.ArticleGuid,
                cancellationToken);

            components.Add(new ArticleKitComponent(fromArticle, componentPack.Quantity));
        }

        var article = Article.CreateArticleKit(command.Name, components, actor);

        articleRepository.Add(article);

        entityEventRepository.Add(EntityEvent.EntityCreated<Article>(article, actor.Guid));

        if (command.Description is { } description)
        {
            article.ChangeDescription(description, actor);
        }

        if (command.ShelfLife is not null)
        {
            article.SetShelfLife(command.ShelfLife, actor);
        }

        if (command.Color is not null)
        {
            article.SetColor(command.Color, actor);
        }

        if (command.Metrics is { } metrics)
        {
            article.SetMetrics(metrics.ToCore(), actor);
        }

        if (command.Barcodes is { } barcodes)
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

        if (command.Partitions is { Count: > 0 } partitions)
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

        if (command.IsActive == false)
        {
            article.Deactivate(actor);

            entityEventRepository.Add(EntityEvent.Deactivated<Article>(article, actor.Guid));
        }

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article {ArticleType} create mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                expectedArticleType,
                article.Guid,
                actor.Guid);
        }

        return article.Guid;
    }
}

/// <summary>
/// Command used to create a container article.
/// </summary>
public sealed record ArticleCreateContainerCommand(
    Name Name,
    Mass? MaxMass = null,
    Description? Description = null,
    TimeSpan? ShelfLife = null,
    Color? Color = null,
    ArticleMetricsDto? Metrics = null,
    ArticleBarcodesDto? Barcodes = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    bool? IsActive = null
) : ICommand<Guid>;

/// <summary>
/// Handles container article creation.
/// </summary>
public sealed class ArticleCreateContainerCommandHandler(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    IEntityPartitionEventRepository entityPartitionEventRepository,
    ArticleService articleService,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<ArticleCreateContainerCommandHandler> logger
) : ICommandHandler<ArticleCreateContainerCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateContainerCommand command,
        CancellationToken cancellationToken = default)
    {
        const ArticleType expectedArticleType = ArticleType.Container;

        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article {ArticleType} create flow started for actor {ActorGuid}.",
                expectedArticleType,
                actor.Guid);
        }

        var article = Article.CreateArticleContainer(command.Name, actor);

        if (command.MaxMass is { } maxMass)
        {
            article.SetContainerMaxMass(maxMass, actor);
        }

        articleRepository.Add(article);

        entityEventRepository.Add(EntityEvent.EntityCreated<Article>(article, actor.Guid));

        if (command.Description is { } description)
        {
            article.ChangeDescription(description, actor);
        }

        if (command.ShelfLife is not null)
        {
            article.SetShelfLife(command.ShelfLife, actor);
        }

        if (command.Color is not null)
        {
            article.SetColor(command.Color, actor);
        }

        if (command.Metrics is { } metrics)
        {
            article.SetMetrics(metrics.ToCore(), actor);
        }

        if (command.Barcodes is { } barcodes)
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

        if (command.Partitions is { Count: > 0 } partitions)
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

        if (command.IsActive == false)
        {
            article.Deactivate(actor);

            entityEventRepository.Add(EntityEvent.Deactivated<Article>(article, actor.Guid));
        }

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article {ArticleType} create mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                expectedArticleType,
                article.Guid,
                actor.Guid);
        }

        return article.Guid;
    }
}

internal static class ArticleCreateRequestValidator
{
    public static void ValidateKitPacks(IReadOnlyCollection<ArticleCreateKitPackDto> packs)
    {
        ArgumentNullException.ThrowIfNull(packs);

        if (packs.Count == 0)
        {
            throw new ArgumentException(
                "Kit article creation requires at least one pack.",
                nameof(packs));
        }
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
