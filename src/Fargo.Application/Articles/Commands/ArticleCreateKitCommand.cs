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
using System.Drawing;

namespace Fargo.Application.Articles.Commands;

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

        if (command.Packs.Count == 0)
        {
            throw new ArgumentException(
                "Kit article creation requires at least one pack.",
                nameof(command));
        }

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
