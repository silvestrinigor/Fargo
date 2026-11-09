using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Operations.Articles.Dtos;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Partitions;


namespace Fargo.Application.Operations.Articles;

/// <summary>
/// Represents a command to update an existing article.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article to update.</param>
/// <param name="Article">The data containing the changes to apply to the article.</param>
public sealed record ArticleUpdateCommand(
    Guid ArticleGuid,
    ArticleUpdateDto Article
) : ICommand;

/// <summary>
/// Handles commands that update existing articles.
/// </summary>
/// <param name="actorService">Resolves the current actor and its permissions.</param>
/// <param name="articleService">Provides article-specific operations and validation.</param>
/// <param name="articleRepository">Provides access to article entities.</param>
/// <param name="partitionRepository">Provides access to partitions referenced by the update.</param>
/// <param name="currentActor">Provides information about the currently authenticated actor.</param>
public sealed class ArticleUpdateCommandHandler(
    ActorResolver actorService, ArticleService articleService,
    IArticleRepository articleRepository, IPartitionRepository partitionRepository,
    ICurrentActor currentActor
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
    public async Task HandleAsync(ArticleUpdateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.EditArticle);

        var article = await articleRepository.GetByGuidAsync(command.ArticleGuid, cancellationToken);

        EntityNotFoundFargoApplicationException.ThrowIfNull(article, command.ArticleGuid);

        actor.ThrowIfAccessDenied(article);

        var articleUpdateDto = command.Article;

        if (articleUpdateDto.Name is { } name)
        {
            article.Name = name;
        }

        if (articleUpdateDto.Description is { } description)
        {
            article.Description = description;
        }

        if (articleUpdateDto.RemoveShelfLife is true)
        {
            article.ShelfLife = null;
        }
        else if (articleUpdateDto.ShelfLife is { } shelfLife)
        {
            article.ShelfLife = shelfLife;
        }

        if (
            articleUpdateDto.RemoveMass is true ||
            articleUpdateDto.Mass is not null ||
            articleUpdateDto.Dimension?.RemoveLengthX is true ||
            articleUpdateDto.Dimension?.LengthX is not null ||
            articleUpdateDto.Dimension?.RemoveLengthY is true ||
            articleUpdateDto.Dimension?.LengthY is not null ||
            articleUpdateDto.Dimension?.RemoveLengthZ is true ||
            articleUpdateDto.Dimension?.LengthZ is not null)
        {
            article.SetMetrics(
                articleUpdateDto.RemoveMass is true
                    ? null : articleUpdateDto.Mass ?? article.Mass,

                articleUpdateDto.Dimension?.RemoveLengthX is true
                    ? null : articleUpdateDto.Dimension?.LengthX ?? article.Dimension.X,

                articleUpdateDto.Dimension?.RemoveLengthY is true
                    ? null : articleUpdateDto.Dimension?.LengthY ?? article.Dimension.Y,

                articleUpdateDto.Dimension?.RemoveLengthZ is true
                    ? null : articleUpdateDto.Dimension?.LengthZ ?? article.Dimension.Z);
        }

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

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                article.AddPartition(partition);
            }
        }

        if (articleUpdateDto.PartitionsToRemove is { Count: > 0 } partitionsToRemove)
        {
            foreach (var partitionGuid in partitionsToRemove.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                article.RemovePartition(partition.Guid);
            }
        }
    }
}
