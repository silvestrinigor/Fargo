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
/// Command used to create a new <see cref="Article"/>.
/// </summary>
public sealed record ArticleCreateCommand(
    ArticleCreateModel Article
    ) : ICommand<Guid>;

/// <summary>
/// Handles <see cref="ArticleCreateCommand"/>.
/// </summary>
public sealed class ArticleCreateCommandHandler(
    ActorService actorService,
    IArticleRepository articleRepository,
    IArticleQueryRepository articleQueryRepository,
    IPartitionRepository partitionRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    IEventRecorder eventRecorder,
    IFargoEventPublisher eventPublisher
    ) : ICommandHandler<ArticleCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(command.Article);

        var actor = await actorService.GetAuthorizedActorByGuid(currentUser.UserGuid, cancellationToken);

        actor.ValidateHasPermission(ActionType.CreateArticle);

        var partitions = await ResolvePartitions(actor, command.Article.Partitions, cancellationToken);

        var article = new Article
        {
            Name = new Name(command.Article.Name),
            Description = command.Article.Description is null ? Description.Empty : new Description(command.Article.Description),
            ShelfLife = command.Article.ShelfLife,
        };

        article.Metrics.Mass = command.Article.Metrics?.Mass.ToUnitsNet();
        article.Metrics.LengthX = command.Article.Metrics?.LengthX.ToUnitsNet();
        article.Metrics.LengthY = command.Article.Metrics?.LengthY.ToUnitsNet();
        article.Metrics.LengthZ = command.Article.Metrics?.LengthZ.ToUnitsNet();

        var barcodes = command.Article.Barcodes.ToDomain();
        if (!barcodes.IsEmpty)
        {
            var conflict = await articleQueryRepository.FindConflictingBarcode(barcodes, article.Guid, cancellationToken);
            if (conflict is { } c)
            {
                throw new ArticleBarcodeAlreadyInUseFargoApplicationException(c.Format, c.Code);
            }
        }

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

        foreach (var partition in partitions)
        {
            article.Partitions.Add(partition);
        }

        if (command.Article.IsActive == false)
        {
            article.Deactivate();
        }

        articleRepository.Add(article);

        await eventRecorder.Record(EventType.ArticleCreated, EntityType.Article, article.Guid, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);
        await eventPublisher.PublishArticleCreated(article.Guid, partitions.Select(p => p.Guid).ToArray(), cancellationToken);

        return article.Guid;
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
}
