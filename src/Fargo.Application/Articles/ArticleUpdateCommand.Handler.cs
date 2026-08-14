using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Fargo.Core.Articles;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

/// <summary>
/// Handles commands that update existing articles.
/// </summary>
/// <param name="actorService">Resolves the current actor and its permissions.</param>
/// <param name="articleService">Provides article-specific operations and validation.</param>
/// <param name="articleRepository">Provides access to article entities.</param>
/// <param name="partitionRepository">Provides access to partitions associated with articles.</param>
/// <param name="currentActor">Provides information about the currently authenticated actor.</param>
/// <param name="unitOfWork">Coordinates persistence of the changes.</param>
/// <param name="logger">Logs the execution of the command.</param>
public sealed class ArticlePatchCommandHandler(
    ActorResolver actorService, ArticleService articleService,
    IArticleRepository articleRepository, IPartitionRepository partitionRepository,
    ICurrentActor currentActor, IUnitOfWork unitOfWork,
    ILogger<ArticlePatchCommandHandler> logger
) : ICommandHandler<ArticleUpdateCommand>
{
    /// <summary>
    /// Applies the requested changes to an article after validating the current
    /// actor's permissions and access to the article and any referenced partitions.
    /// </summary>
    /// <param name="command">
    /// The command containing the identifier of the article to update and the changes to apply.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the operation.
    /// </param>
    /// <exception cref="ActorNotFoundFargoApplicationException">
    /// Thrown when the current actor cannot be found.
    /// </exception>
    /// <exception cref="EntityNotFoundFargoApplicationException">
    /// Thrown when the specified article or a referenced partition cannot be found.
    /// </exception>
    public async Task HandleAsync(
        ArticleUpdateCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.UpdateStarted(command.ArticleGuid, currentActor.Guid);

        var actor = await actorService.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        actor.ThrowIfPermissionDenied(ActionType.EditArticle);

        var article = await articleRepository.GetByGuidAsync(command.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, command.ArticleGuid, EntityType.Article);

        actor.ThrowIfAccessDenied(article);

        var articleUpdateDto = command.Article;

        article.Name = articleUpdateDto.Name ?? article.Name;

        article.Description = articleUpdateDto.Description ?? article.Description;

        article.ShelfLife = articleUpdateDto.RemoveShelfLife is true
            ? null : articleUpdateDto.ShelfLife ?? article.ShelfLife;

        article.SetMetrics(
            articleUpdateDto.RemoveMass is true
                ? null : articleUpdateDto.Mass ?? article.Mass,

            articleUpdateDto.Dimension?.RemoveLengthX is true
                ? null : articleUpdateDto.Dimension?.LengthX ?? article.Dimension.X,

            articleUpdateDto.Dimension?.RemoveLengthY is true
                ? null : articleUpdateDto.Dimension?.LengthY ?? article.Dimension.Y,

            articleUpdateDto.Dimension?.RemoveLengthZ is true
                ? null : articleUpdateDto.Dimension?.LengthZ ?? article.Dimension.Z);

        if (articleUpdateDto.Barcode?.RemoveEan13 is true)
        {
            article.Barcode.Ean13 = null;
        }

        else if (articleUpdateDto.Barcode?.Ean13 is { } ean13)
        {
            await articleService.ValidateEan13IsAvailableAsync(ean13, cancellationToken);

            article.Barcode.Ean13 = ean13;
        }

        if (articleUpdateDto.PartitionsToAdd is { Count: > 0 } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                article.AddPartition(partition);
            }
        }

        if (articleUpdateDto.PartitionsToRemove is { Count: > 0 } partitionsToRemove)
        {
            foreach (var partitionGuid in partitionsToRemove.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid, EntityType.Partition);

                actor.ThrowIfAccessDenied(partition);

                article.RemovePartition(partition.Guid);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UpdateCompleted(article.Guid, actor.Guid);
    }
}
