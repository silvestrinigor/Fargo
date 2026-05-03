using Fargo.Application.Authentication;
using Fargo.Application.Events;
using Fargo.Application.Partitions;
using Fargo.Application.Persistence;
using Fargo.Domain;
using Fargo.Domain.Articles;
using Fargo.Domain.Events;
using Fargo.Domain.Partitions;

namespace Fargo.Application.Articles;

/// <summary>
/// Command used to fully replace an existing article (PUT semantics).
/// </summary>
public sealed record ArticleUpdateCommand(
    Guid ArticleGuid,
    ArticleUpdateModel Article
    ) : ICommand;

/// <summary>
/// Handles <see cref="ArticleUpdateCommand"/>.
/// </summary>
public sealed class ArticleUpdateCommandHandler(
    ActorService actorService,
    IArticleRepository articleRepository,
    IArticleQueryRepository articleQueryRepository,
    IPartitionRepository partitionRepository,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    IEventRecorder eventRecorder,
    IFargoEventPublisher eventPublisher
    ) : ICommandHandler<ArticleUpdateCommand>
{
    public async Task Handle(
        ArticleUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Article);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        var partitions = await ResolvePartitions(actor, command.Article.Partitions, cancellationToken);

        var barcodes = command.Article.Barcodes.ToDomain();
        if (!barcodes.IsEmpty)
        {
            var conflict = await articleQueryRepository.FindConflictingBarcode(barcodes, article.Guid, cancellationToken);
            if (conflict is { } c)
            {
                throw new ArticleBarcodeAlreadyInUseFargoApplicationException(c.Format, c.Code);
            }
        }

        article.Name = new Name(command.Article.Name);
        article.Description = command.Article.Description is null ? Description.Empty : new Description(command.Article.Description);
        article.ShelfLife = command.Article.ShelfLife;

        article.Metrics.Mass = command.Article.Metrics?.Mass.ToUnitsNet();
        article.Metrics.LengthX = command.Article.Metrics?.LengthX.ToUnitsNet();
        article.Metrics.LengthY = command.Article.Metrics?.LengthY.ToUnitsNet();
        article.Metrics.LengthZ = command.Article.Metrics?.LengthZ.ToUnitsNet();

        article.Barcodes.Ean13 = barcodes.Ean13;
        article.Barcodes.Ean8 = barcodes.Ean8;
        article.Barcodes.UpcA = barcodes.UpcA;
        article.Barcodes.UpcE = barcodes.UpcE;
        article.Barcodes.Code128 = barcodes.Code128;
        article.Barcodes.Code39 = barcodes.Code39;
        article.Barcodes.Itf14 = barcodes.Itf14;
        article.Barcodes.Gs1128 = barcodes.Gs1128;
        article.Barcodes.QrCode = barcodes.QrCode;
        article.Barcodes.DataMatrix = barcodes.DataMatrix;

        ReconcilePartitions(article, partitions);

        if (article.IsActive != command.Article.IsActive)
        {
            if (command.Article.IsActive)
            {
                article.Activate();
            }
            else
            {
                article.Deactivate();
            }
        }

        await eventRecorder.Record(EventType.ArticleUpdated, EntityType.Article, article.Guid, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);
        await eventPublisher.PublishArticleUpdated(article.Guid, cancellationToken);
    }

    private async Task<IReadOnlyList<Partition>> ResolvePartitions(
        Actor actor,
        IReadOnlyCollection<Guid>? partitionGuids,
        CancellationToken cancellationToken)
    {
        if (partitionGuids is null || partitionGuids.Count == 0)
        {
            return [];
        }

        var partitions = new List<Partition>(partitionGuids.Count);
        foreach (var partitionGuid in partitionGuids.Distinct())
        {
            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);
            actor.ValidateHasPartitionAccess(partition.Guid);
            partitions.Add(partition);
        }

        return partitions;
    }

    private static void ReconcilePartitions(Article article, IReadOnlyList<Partition> partitions)
    {
        var desired = partitions.Select(p => p.Guid).ToHashSet();

        for (int i = article.Partitions.Count - 1; i >= 0; i--)
        {
            if (!desired.Contains(article.Partitions[i].Guid))
            {
                article.Partitions.RemoveAt(i);
            }
        }

        foreach (var partition in partitions)
        {
            if (!article.Partitions.Any(p => p.Guid == partition.Guid))
            {
                article.Partitions.Add(partition);
            }
        }
    }
}
