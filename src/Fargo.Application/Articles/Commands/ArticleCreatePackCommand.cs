using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Articles;
using Microsoft.Extensions.Logging;
using UnitsNet;
using System.Drawing;

namespace Fargo.Application.Articles.Commands;

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
            article.SetMetrics(metrics.Mass, metrics.LengthX, metrics.LengthY, metrics.LengthZ, actor);
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
