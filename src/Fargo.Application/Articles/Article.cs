using Fargo.Application.Authentication;
using Fargo.Application.Partitions;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Barcodes;
using Fargo.Domain.Partitions;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using UnitsNet;

namespace Fargo.Application.Articles;

public sealed record ArticleDto(
    Guid Guid,
    Name Name,
    Description Description,
    TimeSpan? ShelfLife,
    ArticleMetricsDto Metrics,
    ArticleBarcodesDto Barcodes,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive,
    Guid? EditedByGuid
);

public sealed record ArticleBarcodesDto(
    Ean13? Ean13 = null,
    Ean8? Ean8 = null,
    UpcA? UpcA = null,
    UpcE? UpcE = null,
    Code128? Code128 = null,
    Code39? Code39 = null,
    Itf14? Itf14 = null,
    Gs1128? Gs1128 = null,
    QrCode? QrCode = null,
    DataMatrix? DataMatrix = null
);

public sealed record ArticleMetricsDto(
    Mass? Mass = null,
    Length? LengthX = null,
    Length? LengthY = null,
    Length? LengthZ = null
);

public sealed record ArticleBarcodeDto(string Barcode, BarcodeFormat Type);

public static class ArticleDtoMappings
{
    public static readonly Expression<Func<Article, ArticleDto>> Projection = article => new ArticleDto(
        article.Guid,
        article.Name,
        article.Description,
        article.ShelfLife,
        new ArticleMetricsDto(
            article.Metrics.Mass,
            article.Metrics.LengthX,
            article.Metrics.LengthY,
            article.Metrics.LengthZ),
        new ArticleBarcodesDto(
            article.Ean13,
            article.Ean8,
            article.UpcA,
            article.UpcE,
            article.Code128,
            article.Code39,
            article.Itf14,
            article.Gs1128,
            article.QrCode,
            article.DataMatrix),
        article.Partitions.Select(partition => partition.Guid).ToArray(),
        article.IsActive,
        article.EditedByGuid);
}

/// <summary>
/// Exception thrown when an article with the specified identifier cannot be found.
/// </summary>
public class ArticleNotFoundFargoApplicationException(Guid articleGuid)
    : FargoApplicationException($"Article with guid '{articleGuid}' was not found.")
{
    /// <summary>
    /// Gets the identifier of the article that could not be found.
    /// </summary>
    public Guid ArticleGuid { get; } = articleGuid;
}

#region Create

public sealed record ArticleCreateDto(
    Name Name,
    Description? Description = null,
    TimeSpan? ShelfLife = null,
    ArticleMetricsDto? Metrics = null,
    ArticleBarcodesDto? Barcodes = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    bool? IsActive = null
);

public sealed record ArticleCreateCommand(
    ArticleCreateDto Article
) : ICommand<Guid>;

public sealed class ArticleCreateCommandHandler(
    ActorService actorService,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    ILogger<ArticleCreateCommandHandler> logger
) : ICommandHandler<ArticleCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Article create flow started for actor {ActorGuid}.", actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateArticle);

        var article = new Article
        {
            Name = command.Article.Name,
            Description = command.Article.Description ?? Description.Empty,
            ShelfLife = command.Article.ShelfLife
        };

        #region Metrics

        if (command.Article.Metrics is { } metrics)
        {
            article.Metrics = new ArticleMetrics
            {
                Mass = metrics.Mass,
                LengthX = metrics.LengthX,
                LengthY = metrics.LengthY,
                LengthZ = metrics.LengthZ
            };
        }

        #endregion Metrics

        #region Barcode

        if (command.Article.Barcodes is { } barcodes)
        {
            if (barcodes.Ean13 is { } ean13 && await articleRepository.ExistsByBarcode(ean13))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.Ean13);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Ean13, ean13);
            }

            article.Ean13 = barcodes.Ean13;

            if (barcodes.Ean8 is { } ean8 && await articleRepository.ExistsByBarcode(ean8))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.Ean8);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Ean8, ean8);
            }

            article.Ean8 = barcodes.Ean8;

            if (barcodes.UpcA is { } upcA && await articleRepository.ExistsByBarcode(upcA))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.UpcA);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.UpcA, upcA);
            }

            article.UpcA = barcodes.UpcA;

            if (barcodes.UpcE is { } upcE && await articleRepository.ExistsByBarcode(upcE))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.UpcE);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.UpcE, upcE);
            }

            article.UpcE = barcodes.UpcE;

            if (barcodes.Code128 is { } code128 && await articleRepository.ExistsByBarcode(code128))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.Code128);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Code128, code128);
            }

            article.Code128 = barcodes.Code128;

            if (barcodes.Code39 is { } code39 && await articleRepository.ExistsByBarcode(code39))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.Code39);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Code39, code39);
            }

            article.Code39 = barcodes.Code39;

            if (barcodes.Itf14 is { } itf14 && await articleRepository.ExistsByBarcode(itf14))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.Itf14);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Itf14, itf14);
            }

            article.Itf14 = barcodes.Itf14;

            if (barcodes.Gs1128 is { } gs1128 && await articleRepository.ExistsByBarcode(gs1128))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.Gs1128);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Gs1128, gs1128);
            }

            article.Gs1128 = barcodes.Gs1128;

            if (barcodes.QrCode is { } qrCode && await articleRepository.ExistsByBarcode(qrCode))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.QrCode);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.QrCode, qrCode);
            }

            article.QrCode = barcodes.QrCode;

            if (barcodes.DataMatrix is { } dataMatrix && await articleRepository.ExistsByBarcode(dataMatrix))
            {
                logger.LogWarning("Article create flow rejected because barcode type {BarcodeType} is already in use.", BarcodeFormat.DataMatrix);
                throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.DataMatrix, dataMatrix);
            }

            article.DataMatrix = barcodes.DataMatrix;
        }

        #endregion Barcode

        #region Partition

        foreach (var partitionGuid in command.Article.Partitions ?? [])
        {
            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);

            article.Partitions.Add(partition);
        }

        #endregion Partition

        articleRepository.Add(article);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article create flow completed for article {ArticleGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}.",
                article.Guid,
                actor.Guid,
                article.Partitions.Count);
        }

        return article.Guid;
    }
}

#endregion Create

#region Delete

public sealed record ArticleDeleteCommand(
    Guid ArticleGuid
) : ICommand;

public sealed class ArticleDeleteCommandHandler(
    ActorService actorService,
    IArticleRepository articleRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<ArticleDeleteCommandHandler> logger
) : ICommandHandler<ArticleDeleteCommand>
{
    public async Task Handle(
        ArticleDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article delete flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.DeleteArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        var hasItems = await articleRepository.HasItemsAssociated(
            article.Guid,
            cancellationToken
        );

        if (hasItems)
        {
            logger.LogWarning(
                "Article delete flow rejected because article {ArticleGuid} has associated items.",
                article.Guid);
            throw new ArticleDeleteWithItemsAssociatedFargoDomainException(article.Guid);
        }

        articleRepository.Remove(article);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article delete flow completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }
    }
}

#endregion Delete

#region Update

public sealed record ArticleUpdateDto(
    Name Name,
    Description Description,
    TimeSpan? ShelfLife,
    ArticleMetricsDto Metrics,
    ArticleBarcodesDto Barcodes,
    IReadOnlyCollection<Guid> Partitions,
    bool IsActive
);

public sealed record ArticleUpdateCommand(
    Guid ArticleGuid,
    ArticleUpdateDto Article
) : ICommand;

public sealed class ArticleUpdateCommandHandler(
    ActorService actorService,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    ILogger<ArticleUpdateCommandHandler> logger
) : ICommandHandler<ArticleUpdateCommand>
{
    public async Task Handle(
        ArticleUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article update flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        if (article.Name != command.Article.Name)
        {
            article.Name = command.Article.Name;
        }

        if (article.Description != command.Article.Description)
        {
            article.Description = command.Article.Description;
        }

        if (article.ShelfLife != command.Article.ShelfLife)
        {
            article.ShelfLife = command.Article.ShelfLife;
        }

        if (command.Article.Metrics is { } metrics)
        {
            if (!article.Metrics.Mass.Equals(metrics.Mass))
            {
                article.Metrics.Mass = metrics.Mass;
            }

            if (!article.Metrics.LengthX.Equals(metrics.LengthX))
            {
                article.Metrics.LengthX = metrics.LengthX;
            }

            if (!article.Metrics.LengthY.Equals(metrics.LengthY))
            {
                article.Metrics.LengthY = metrics.LengthY;
            }

            if (!article.Metrics.LengthZ.Equals(metrics.LengthZ))
            {
                article.Metrics.LengthZ = metrics.LengthZ;
            }
        }

        if (command.Article.Barcodes is { } barcodes)
        {
            if (barcodes.Ean13 != article.Ean13)
            {
                if (barcodes.Ean13 is { } ean13 && await articleRepository.ExistsByBarcode(ean13))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.Ean13, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Ean13, ean13);
                }

                article.Ean13 = barcodes.Ean13;
            }

            if (barcodes.Ean8 != article.Ean8)
            {
                if (barcodes.Ean8 is { } ean8 && await articleRepository.ExistsByBarcode(ean8))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.Ean8, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Ean8, ean8);
                }

                article.Ean8 = barcodes.Ean8;
            }

            if (barcodes.UpcA != article.UpcA)
            {
                if (barcodes.UpcA is { } upcA && await articleRepository.ExistsByBarcode(upcA))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.UpcA, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.UpcA, upcA);
                }

                article.UpcA = barcodes.UpcA;
            }

            if (barcodes.UpcE != article.UpcE)
            {
                if (barcodes.UpcE is { } upcE && await articleRepository.ExistsByBarcode(upcE))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.UpcE, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.UpcE, upcE);
                }

                article.UpcE = barcodes.UpcE;
            }

            if (barcodes.Code128 != article.Code128)
            {
                if (barcodes.Code128 is { } code128 && await articleRepository.ExistsByBarcode(code128))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.Code128, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Code128, code128);
                }

                article.Code128 = barcodes.Code128;
            }

            if (barcodes.Code39 != article.Code39)
            {
                if (barcodes.Code39 is { } code39 && await articleRepository.ExistsByBarcode(code39))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.Code39, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Code39, code39);
                }

                article.Code39 = barcodes.Code39;
            }

            if (barcodes.Itf14 != article.Itf14)
            {
                if (barcodes.Itf14 is { } itf14 && await articleRepository.ExistsByBarcode(itf14))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.Itf14, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Itf14, itf14);
                }

                article.Itf14 = barcodes.Itf14;
            }

            if (barcodes.Gs1128 != article.Gs1128)
            {
                if (barcodes.Gs1128 is { } gs1128 && await articleRepository.ExistsByBarcode(gs1128))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.Gs1128, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.Gs1128, gs1128);
                }

                article.Gs1128 = barcodes.Gs1128;
            }

            if (barcodes.QrCode != article.QrCode)
            {
                if (barcodes.QrCode is { } qrCode && await articleRepository.ExistsByBarcode(qrCode))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.QrCode, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.QrCode, qrCode);
                }

                article.QrCode = barcodes.QrCode;
            }

            if (barcodes.DataMatrix != article.DataMatrix)
            {
                if (barcodes.DataMatrix is { } dataMatrix && await articleRepository.ExistsByBarcode(dataMatrix))
                {
                    logger.LogWarning("Article update flow rejected because barcode type {BarcodeType} is already in use for article {ArticleGuid}.", BarcodeFormat.DataMatrix, article.Guid);
                    throw new ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat.DataMatrix, dataMatrix);
                }

                article.DataMatrix = barcodes.DataMatrix;
            }
        }

        if (command.Article.Partitions is { } requestedPartitions)
        {
            foreach (var partitionGuid in requestedPartitions)
            {
                if (article.Partitions.Any(p => p.Guid == partitionGuid))
                {
                    continue;
                }

                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                actor.ValidateHasPartitionAccess(partition.Guid);

                article.Partitions.Add(partition);
            }

            var partitionsToRemove = article.Partitions
                .Where(p => !requestedPartitions.Contains(p.Guid))
                .ToList();

            foreach (var partition in partitionsToRemove)
            {
                actor.ValidateHasPartitionAccess(partition.Guid);

                article.Partitions.Remove(partition);
            }
        }

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article update flow completed for article {ArticleGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}.",
                article.Guid,
                actor.Guid,
                article.Partitions.Count);
        }
    }
}

#endregion Update

#region Single

public sealed record ArticleSingleQuery(
    Guid ArticleGuid,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ArticleDto?>;

public sealed class ArticleSingleQueryHandler(
    ActorService actorService,
    IArticleQueryRepository articleRepository,
    ICurrentUser currentUser,
    ILogger<ArticleSingleQueryHandler> logger
) : IQueryHandler<ArticleSingleQuery, ArticleDto?>
{
    public async Task<ArticleDto?> Handle(
        ArticleSingleQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Article single query started for article {ArticleGuid} by actor {ActorGuid}.",
                query.ArticleGuid,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        var article = await articleRepository.GetInfoByGuid(
            query.ArticleGuid,
            query.AsOfDateTime,
            actor.PartitionAccessesGuids,
            notChildOfAnyPartition: true,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Article single query completed for article {ArticleGuid} by actor {ActorGuid}. Found: {Found}.",
                query.ArticleGuid,
                actor.Guid,
                article is not null);
        }

        return article;
    }
}

#endregion Single

#region Barcode

public sealed record ArticleByBarcodeQuery(
    ArticleBarcodeDto ArticleBarcode,
    DateTimeOffset? AsOfDateTime = null
) : IQuery<ArticleDto?>;

public sealed class ArticleByBarcodeQueryHandler(
    ActorService actorService,
    IArticleQueryRepository articleRepository,
    ICurrentUser currentUser,
    ILogger<ArticleByBarcodeQueryHandler> logger
) : IQueryHandler<ArticleByBarcodeQuery, ArticleDto?>
{
    public async Task<ArticleDto?> Handle(
        ArticleByBarcodeQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actorGuid = currentUser.UserGuid;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Article barcode query started for barcode type {BarcodeType} by actor {ActorGuid}.",
                query.ArticleBarcode.Type,
                actorGuid);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        var article = await articleRepository.GetInfoByBarcode(
            query.ArticleBarcode,
            query.AsOfDateTime,
            actor.PartitionAccessesGuids,
            notChildOfAnyPartition: true,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Article barcode query completed for barcode type {BarcodeType} by actor {ActorGuid}. Found: {Found}.",
                query.ArticleBarcode.Type,
                actor.Guid,
                article is not null);
        }

        return article;
    }
}

#endregion Barcode

#region Many

public sealed record ArticlesQuery(
    Pagination WithPagination,
    DateTimeOffset? TemporalAsOfDateTime = null,
    IReadOnlyCollection<Guid>? ChildOfAnyOfThesePartitions = null,
    bool? NotChildOfAnyPartition = null
) : IQuery<IReadOnlyCollection<ArticleDto>>;

public sealed class ArticlesQueryHandler(
    ActorService actorService,
    IArticleQueryRepository articleRepository,
    ICurrentUser currentUser,
    ILogger<ArticlesQueryHandler> logger
) : IQueryHandler<ArticlesQuery, IReadOnlyCollection<ArticleDto>>
{
    public async Task<IReadOnlyCollection<ArticleDto>> Handle(
        ArticlesQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var actorGuid = currentUser.UserGuid;
        var pagination = query.WithPagination;

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Articles query started for actor {ActorGuid}. Page: {Page}. Limit: {Limit}.",
                actorGuid,
                pagination.Page,
                pagination.Limit);
        }

        var actor = await actorService.GetAuthorizedActorByGuid(actorGuid, cancellationToken);

        var (childOfAnyOfThesePartitions, notChildOfAnyPartition) =
            PartitionQueryFilter.ForPartitionedEntities(
                actor.PartitionAccessesGuids,
                query.ChildOfAnyOfThesePartitions,
                query.NotChildOfAnyPartition);

        var articles = await articleRepository.GetManyInfo(
            pagination,
            query.TemporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition,
            cancellationToken
        );

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Articles query completed for actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}. EffectivePartitionCount: {EffectivePartitionCount}. ResultCount: {ResultCount}.",
                actor.Guid,
                query.ChildOfAnyOfThesePartitions?.Count ?? 0,
                childOfAnyOfThesePartitions?.Count ?? 0,
                articles.Count);
        }

        return articles;
    }
}

#endregion Many

#region Repositories

public interface IArticleQueryRepository
{
    Task<ArticleDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );

    Task<ArticleDto?> GetInfoByBarcode(
        ArticleBarcodeDto articleBarcode,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ArticleDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );
}

public static class ArticleRepositoryExtensions
{
    extension(IArticleRepository repository)
    {
        public async Task<Article> GetFoundByGuid(
            Guid articleGuid,
            CancellationToken cancellationToken = default
        )
        {
            var article = await repository.GetByGuid(articleGuid, cancellationToken)
                ?? throw new ArticleNotFoundFargoApplicationException(articleGuid);

            return article;
        }
    }
}

#endregion Repositories
