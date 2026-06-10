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
    IEventRepository entityEventRepository,
    IPartitionEventRepository entityPartitionEventRepository,
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
            await articleService.SetEan13(barcodes.Ean13, article, actor, cancellationToken);
            await articleService.SetEan8(barcodes.Ean8, article, actor, cancellationToken);
            await articleService.SetUpcA(barcodes.UpcA, article, actor, cancellationToken);
            await articleService.SetUpcE(barcodes.UpcE, article, actor, cancellationToken);
            await articleService.SetCode128(barcodes.Code128, article, actor, cancellationToken);
            await articleService.SetCode39(barcodes.Code39, article, actor, cancellationToken);
            await articleService.SetItf14(barcodes.Itf14, article, actor, cancellationToken);
            await articleService.SetGs1128(barcodes.Gs1128, article, actor, cancellationToken);
            await articleService.SetQrCode(barcodes.QrCode, article, actor, cancellationToken);
            await articleService.SetDataMatrix(barcodes.DataMatrix, article, actor, cancellationToken);
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

                entityPartitionEventRepository.Add(PartitionEvent.InsertedIntoPartition(
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

                entityPartitionEventRepository.Add(PartitionEvent.RemovedFromPartition(
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
                entityEventRepository.Add(Event.Activated<Article>(article, actor.Guid));
            }
            else if (!isActive && article.IsActive)
            {
                article.Deactivate(actor);
                entityEventRepository.Add(Event.Deactivated<Article>(article, actor.Guid));
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
