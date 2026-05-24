using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Identity;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

#region Create

/// <summary>
/// Command used to create a default article from an API creation payload.
/// </summary>
public sealed record ArticleCreateDefaultCommand(
    ArticleCreateDto Create
) : ICommand<Guid>;

/// <summary>
/// Command used to create a variation article from an API creation payload.
/// </summary>
public sealed record ArticleCreateVariationCommand(
    ArticleCreateDto Create
) : ICommand<Guid>;

/// <summary>
/// Command used to create a pack article from an API creation payload.
/// </summary>
public sealed record ArticleCreatePackCommand(
    ArticleCreateDto Create
) : ICommand<Guid>;

/// <summary>
/// Command used to create a kit article from an API creation payload.
/// </summary>
public sealed record ArticleCreateKitCommand(
    ArticleCreateDto Create
) : ICommand<Guid>;

/// <summary>
/// Command used to create a container article from an API creation payload.
/// </summary>
public sealed record ArticleCreateContainerCommand(
    ArticleCreateDto Create
) : ICommand<Guid>;

public abstract class ArticleCreateCommandHandlerBase(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IEntityEventRepository entityEventRepository,
    IEntityPartitionEventRepository entityPartitionEventRepository,
    ArticleService articleService,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork
)
{
    protected IArticleRepository ArticleRepository { get; } = articleRepository;

    protected async Task<Guid> HandleCreate(
        ArticleCreateDto create,
        ArticleType expectedArticleType,
        Func<ArticleCreateDto, Actor, CancellationToken, Task<Article>> articleFactory,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        ArticleCreateRequestValidator.Validate(create, expectedArticleType);

        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article {ArticleType} create flow started for actor {ActorGuid}.",
                expectedArticleType,
                actor.Guid);
        }

        var article = await articleFactory(create, actor, cancellationToken);

        ArticleRepository.Add(article);

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
                "Article {ArticleType} create mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                expectedArticleType,
                article.Guid,
                actor.Guid);
        }

        return article.Guid;
    }
}

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
) : ArticleCreateCommandHandlerBase(
        articleRepository,
        partitionRepository,
        entityEventRepository,
        entityPartitionEventRepository,
        articleService,
        currentAuthorizationContext,
        unitOfWork),
    ICommandHandler<ArticleCreateDefaultCommand, Guid>
{
    public Task<Guid> Handle(
        ArticleCreateDefaultCommand command,
        CancellationToken cancellationToken = default)
        => HandleCreate(
            command.Create,
            ArticleType.Default,
            (create, actor, _) => Task.FromResult(Article.CreateArticle(create.Name, actor)),
            logger,
            cancellationToken);
}

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
) : ArticleCreateCommandHandlerBase(
        articleRepository,
        partitionRepository,
        entityEventRepository,
        entityPartitionEventRepository,
        articleService,
        currentAuthorizationContext,
        unitOfWork),
    ICommandHandler<ArticleCreateVariationCommand, Guid>
{
    public Task<Guid> Handle(
        ArticleCreateVariationCommand command,
        CancellationToken cancellationToken = default)
        => HandleCreate(
            command.Create,
            ArticleType.Variation,
            async (create, actor, ct) =>
            {
                var fromArticle = await ArticleRepository.GetFoundByGuid(
                    create.Variation!.FromArticleGuid,
                    ct);

                return Article.CreateArticleVariation(create.Name, fromArticle, actor);
            },
            logger,
            cancellationToken);
}

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
) : ArticleCreateCommandHandlerBase(
        articleRepository,
        partitionRepository,
        entityEventRepository,
        entityPartitionEventRepository,
        articleService,
        currentAuthorizationContext,
        unitOfWork),
    ICommandHandler<ArticleCreatePackCommand, Guid>
{
    public Task<Guid> Handle(
        ArticleCreatePackCommand command,
        CancellationToken cancellationToken = default)
        => HandleCreate(
            command.Create,
            ArticleType.Pack,
            async (create, actor, ct) =>
            {
                var fromArticle = await ArticleRepository.GetFoundByGuid(
                    create.Pack!.FromArticleGuid,
                    ct);

                return Article.CreateArticlePack(create.Name, fromArticle, create.Pack.Quantity, actor);
            },
            logger,
            cancellationToken);
}

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
) : ArticleCreateCommandHandlerBase(
        articleRepository,
        partitionRepository,
        entityEventRepository,
        entityPartitionEventRepository,
        articleService,
        currentAuthorizationContext,
        unitOfWork),
    ICommandHandler<ArticleCreateKitCommand, Guid>
{
    public Task<Guid> Handle(
        ArticleCreateKitCommand command,
        CancellationToken cancellationToken = default)
        => HandleCreate(
            command.Create,
            ArticleType.Kit,
            async (create, actor, ct) =>
            {
                var kit = create.Kit!;
                var components = new List<ArticleKitComponent>(kit.Packs.Count);

                foreach (var componentPack in kit.Packs)
                {
                    var fromArticle = await ArticleRepository.GetFoundByGuid(
                        componentPack.ArticleGuid,
                        ct);

                    components.Add(new ArticleKitComponent(fromArticle, componentPack.Quantity));
                }

                return Article.CreateArticleKit(create.Name, components, actor);
            },
            logger,
            cancellationToken);
}

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
) : ArticleCreateCommandHandlerBase(
        articleRepository,
        partitionRepository,
        entityEventRepository,
        entityPartitionEventRepository,
        articleService,
        currentAuthorizationContext,
        unitOfWork),
    ICommandHandler<ArticleCreateContainerCommand, Guid>
{
    public Task<Guid> Handle(
        ArticleCreateContainerCommand command,
        CancellationToken cancellationToken = default)
        => HandleCreate(
            command.Create,
            ArticleType.Container,
            (create, actor, _) =>
            {
                var article = Article.CreateArticleContainer(create.Name, actor);

                if (create.Container?.MaxMass is { } maxMass)
                {
                    article.SetContainerMaxMass(maxMass, actor);
                }

                return Task.FromResult(article);
            },
            logger,
            cancellationToken);
}

internal static class ArticleCreateRequestValidator
{
    public static void Validate(ArticleCreateDto create, ArticleType expectedArticleType)
    {
        if (!Enum.IsDefined(create.ArticleType))
        {
            throw new ArgumentException(
                $"Unsupported article type '{create.ArticleType}'.",
                nameof(create));
        }

        if (create.ArticleType != expectedArticleType)
        {
            throw new ArgumentException(
                $"Article creation command for '{expectedArticleType}' cannot create '{create.ArticleType}' articles.",
                nameof(create));
        }

        switch (expectedArticleType)
        {
            case ArticleType.Default:
                RejectPayload(
                    create.Variation is not null,
                    create.Pack is not null,
                    create.Kit is not null,
                    create.Container is not null,
                    expectedArticleType,
                    create);
                break;
            case ArticleType.Variation:
                RequirePayload(create.Variation is not null, expectedArticleType, create);
                RejectPayload(
                    false,
                    create.Pack is not null,
                    create.Kit is not null,
                    create.Container is not null,
                    expectedArticleType,
                    create);
                break;
            case ArticleType.Pack:
                RequirePayload(create.Pack is not null, expectedArticleType, create);
                RejectPayload(
                    create.Variation is not null,
                    false,
                    create.Kit is not null,
                    create.Container is not null,
                    expectedArticleType,
                    create);
                break;
            case ArticleType.Kit:
                RequirePayload(create.Kit is not null, expectedArticleType, create);
                RejectPayload(
                    create.Variation is not null,
                    create.Pack is not null,
                    false,
                    create.Container is not null,
                    expectedArticleType,
                    create);

                if (create.Kit!.Packs.Count == 0)
                {
                    throw new ArgumentException(
                        "Kit article creation requires at least one pack.",
                        nameof(create));
                }

                break;
            case ArticleType.Container:
                RejectPayload(
                    create.Variation is not null,
                    create.Pack is not null,
                    create.Kit is not null,
                    false,
                    expectedArticleType,
                    create);
                break;
            default:
                throw new ArgumentException(
                    $"Unsupported article type '{expectedArticleType}'.",
                    nameof(expectedArticleType));
        }
    }

    private static void RequirePayload(
        bool payloadProvided,
        ArticleType expectedArticleType,
        ArticleCreateDto create)
    {
        if (!payloadProvided)
        {
            throw new ArgumentException(
                $"Article type '{expectedArticleType}' requires its matching create payload.",
                nameof(create));
        }
    }

    private static void RejectPayload(
        bool variationProvided,
        bool packProvided,
        bool kitProvided,
        bool containerProvided,
        ArticleType expectedArticleType,
        ArticleCreateDto create)
    {
        if (variationProvided || packProvided || kitProvided || containerProvided)
        {
            throw new ArgumentException(
                $"Article type '{expectedArticleType}' cannot include payloads for another article type.",
                nameof(create));
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
