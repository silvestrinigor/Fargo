using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Dtos.Articles;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;

namespace Fargo.Application.Commands.Articles;

public sealed record ArticleUpdateCommand(
    Guid ArticleGuid,
    ArticleUpdateDto Article
) : ICommand;

public sealed class ArticleUpdateCommandHandler(
    ActorResolver actorService, ArticleService articleService,
    IArticleRepository articleRepository,
    ICurrentActor currentActor
) : ICommandHandler<ArticleUpdateCommand>
{
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
    }
}
