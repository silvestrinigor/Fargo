using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Articles;
using Fargo.Core.Events;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles.Commands;

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

        if (patch.RemoveShelfLife is true)
        {
            article.SetShelfLife(null, actor);
        }

        if (patch.ShelfLife is not null)
        {
            article.SetShelfLife(patch.ShelfLife.Value, actor);
        }

        if (patch.Metrics is { } metrics)
        {
            article.SetMetrics(metrics.Mass, metrics.LengthX, metrics.LengthY, metrics.LengthZ, actor);
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
