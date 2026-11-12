using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Exceptions;
using Fargo.Application.Operations.Articles.Dtos;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;
using Fargo.Core.Entities.Articles;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using UnitsNet;

namespace Fargo.Application.Operations.Articles;

public sealed record ArticleCreateCommand(
    ArticleCreateDto Create
) : ICommand<Guid>;

public sealed class ArticleCreateCommandHandler(
    ArticleService articleService, ActorResolver actorService,
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ICurrentActor currentActor
    ) : ICommandHandler<ArticleCreateCommand, Guid>
{
    public async Task<Guid> HandleAsync(ArticleCreateCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await actorService.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        actor.ThrowIfPermissionDenied(ActionType.CreateArticle);

        Article article;

        switch (command.Create.ArticleType)
        {
            case ArticleType.Default:
                {
                    article = Article.NewArticle(command.Create.Name, command.Create.TrackingMode);

                    break;
                }

            case ArticleType.Variation:
                {
                    if (command.Create.Variation?.FromArticleGuid is null)
                    {
                        throw new FargoApplicationException(
                            "Variation from article guid must be informed when the article type is variation.", FargoErrorType.InvalidOperation);
                    }

                    var fromArticle = await articleRepository.GetByGuidAsync(command.Create.Variation.FromArticleGuid, cancellationToken);

                    EntityNotFoundFargoApplicationException.ThrowIfNull(fromArticle, command.Create.Variation.FromArticleGuid);

                    actor.ThrowIfAccessDenied(fromArticle);

                    article = Article.NewArticleVariation(command.Create.Name, fromArticle, command.Create.TrackingMode);

                    break;
                }

            case ArticleType.Pack:
                {
                    if (command.Create.Pack?.FromArticleGuid is null)
                    {
                        throw new FargoApplicationException(
                            "Pack from article guid must be informed when the article type is pack.", FargoErrorType.InvalidOperation);
                    }

                    if (command.Create.Pack?.Quantity is null)
                    {
                        throw new FargoApplicationException(
                            "Pack quantity should be informed when article type is pack.", FargoErrorType.InvalidOperation);
                    }

                    var fromArticle = await articleRepository.GetByGuidAsync(command.Create.Pack!.FromArticleGuid, cancellationToken);

                    EntityNotFoundFargoApplicationException.ThrowIfNull(fromArticle, command.Create.Pack.FromArticleGuid);

                    actor.ThrowIfAccessDenied(fromArticle);

                    article = Article.NewArticlePack(command.Create.Name, fromArticle, command.Create.Pack.Quantity, command.Create.TrackingMode);

                    break;
                }

            case ArticleType.Kit:
                {
                    if (command.Create.KitComponents is null || command.Create.KitComponents.Count == 0)
                    {
                        throw new FargoApplicationException(
                            "Kit components should be informed when article type is kit.", FargoErrorType.InvalidOperation);
                    }

                    var kitComponents = new List<(Article, Scalar)>();

                    foreach (var kdo in command.Create.KitComponents!)
                    {
                        var fromArticle = await articleRepository.GetByGuidAsync(kdo.ArticleGuid, cancellationToken);

                        EntityNotFoundFargoApplicationException.ThrowIfNull(fromArticle, kdo.ArticleGuid);

                        actor.ThrowIfAccessDenied(fromArticle);

                        var kit = (fromArticle, kdo.Quantity);

                        kitComponents.Add(kit);
                    }

                    article = Article.NewArticleKit(command.Create.Name, kitComponents, command.Create.TrackingMode);

                    break;
                }

            case ArticleType.Container:
                {
                    var allowedArticles = new List<(Article Article, Scalar? MaxQuantity)>();

                    if (command.Create.Container?.AllowedArticles is { Count: > 0 } allowedArticleDtos)
                    {
                        foreach (var allowed in allowedArticleDtos)
                        {
                            var allowedArticle = await articleRepository.GetByGuidAsync(allowed.ArticleGuid, cancellationToken);

                            EntityNotFoundFargoApplicationException.ThrowIfNull(allowedArticle, allowed.ArticleGuid);

                            actor.ThrowIfAccessDenied(allowedArticle);

                            allowedArticles.Add((allowedArticle, allowed.MaxQuantity));
                        }
                    }

                    article = Article.NewArticleContainer(
                        command.Create.Name,
                        command.Create.Container?.MaxMass,
                        allowedArticles);

                    break;
                }

            default:
                throw new FargoApplicationException("Article type not supported.", FargoErrorType.InvalidOperation);
        }

        article.Description = command.Create.Description ?? Description.Empty;

        article.ShelfLife = command.Create.ShelfLife;

        article.Color = command.Create.Color ?? null;

        article.SetMetrics(
            command.Create.Mass ?? null,

            command.Create.Dimension?.LengthX ?? null,

            command.Create.Dimension?.LengthY ?? null,

            command.Create.Dimension?.LengthZ ?? null
        );

        if (command.Create.Barcode?.Ean13 is { } ean13)
        {
            await articleService.ValidateEan13IsAvailableAsync(ean13, cancellationToken);

            article.Barcode.Ean13 = ean13;
        }

        if (command.Create.PartitionsToAdd is { Count: > 0 } partitionsToAdd)
        {
            foreach (var partitionGuid in partitionsToAdd.Distinct())
            {
                var partition = await partitionRepository.GetByGuidAsync(partitionGuid, cancellationToken);

                EntityNotFoundFargoApplicationException.ThrowIfNull(partition, partitionGuid);

                actor.ThrowIfAccessDenied(partition);

                article.AddPartition(partition);
            }
        }

        articleRepository.Add(article);

        return article.Guid;
    }
}
