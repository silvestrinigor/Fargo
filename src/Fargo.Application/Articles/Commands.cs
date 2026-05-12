using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Articles;

#region Create

public sealed record ArticleCreateCommand(
    ArticleCreateDto Article
) : ICommand<Guid>;

public sealed class ArticleCreateCommandHandler(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ArticleService articleService,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<ArticleCreateCommandHandler> logger
) : ICommandHandler<ArticleCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Article create flow started for actor {ActorGuid}.", actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.CreateArticle);

        var article = new Article
        {
            Name = command.Article.Name,
            Description = command.Article.Description ?? Description.Empty,
            ShelfLife = command.Article.ShelfLife,
            Color = command.Article.Color
        };

        if (command.Article.Metrics is { } metrics)
        {
            article.Mass = metrics.Mass;
            article.LengthX = metrics.LengthX;
            article.LengthY = metrics.LengthY;
            article.LengthZ = metrics.LengthZ;
        }

        // TODO: Move this to inside a ArticleBarcodesDto to only edit the article barcodes if the dto is not null because the way it is can have some problems when more than one client tries to edit the entity at the same time.
        await articleService.SetEan13(article, command.Article.Ean13, cancellationToken);

        await articleService.SetEan8(article, command.Article.Ean8, cancellationToken);

        await articleService.SetUpcA(article, command.Article.UpcA, cancellationToken);

        await articleService.SetUpcE(article, command.Article.UpcE, cancellationToken);

        await articleService.SetCode128(article, command.Article.Code128, cancellationToken);

        await articleService.SetCode39(article, command.Article.Code39, cancellationToken);

        await articleService.SetItf14(article, command.Article.Itf14, cancellationToken);

        await articleService.SetGs1128(article, command.Article.Gs1128, cancellationToken);

        await articleService.SetQrCode(article, command.Article.QrCode, cancellationToken);

        await articleService.SetDataMatrix(article, command.Article.DataMatrix, cancellationToken);

        foreach (var partitionGuid in command.Article.Partitions ?? [])
        {
            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);

            article.Partitions.Add(partition);
        }

        articleRepository.Add(article);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article create flow completed for article {ArticleGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}.",
                article.Guid,
                actor.ActorGuid,
                article.Partitions.Count);
        }

        return article.Guid;
    }
}

#endregion Create

#region Delete

public sealed record ArticleDeleteCommand(
    Guid ArticleGuid
) : ICommand;

public sealed class ArticleDeleteCommandHandler(
    IArticleRepository articleRepository,
    IUnitOfWork unitOfWork,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleDeleteCommandHandler> logger
) : ICommandHandler<ArticleDeleteCommand>
{
    public async Task Handle(
        ArticleDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article delete flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.DeleteArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        // TODO: move this validation to a domain service.
        var hasItems = await articleRepository.HasItemsAssociated(
            article.Guid,
            cancellationToken
        );

        if (hasItems)
        {
            logger.LogWarning(
                "Article delete flow rejected because article {ArticleGuid} has associated items.",
                article.Guid);
            throw new ArticleDeleteWithItemsAssociatedFargoDomainException(article.Guid);
        }

        articleRepository.Remove(article);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article delete flow completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.ActorGuid);
        }
    }
}

#endregion Delete

#region Update

public sealed record ArticleUpdateCommand(
    Guid ArticleGuid,
    ArticleUpdateDto Article
) : ICommand;

public sealed class ArticleUpdateCommandHandler(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ArticleService articleService,
    IUnitOfWork unitOfWork,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleUpdateCommandHandler> logger
) : ICommandHandler<ArticleUpdateCommand>
{
    public async Task Handle(
        ArticleUpdateCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var actor = await currentAuthorizationContext.GetAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article update flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.ActorGuid);
        }

        actor.ValidateHasPermission(ActionType.EditArticle);

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        actor.ValidateHasAccess(article);

        if (article.Name != command.Article.Name)
        {
            article.Name = command.Article.Name;
        }

        if (article.Description != command.Article.Description)
        {
            article.Description = command.Article.Description;
        }

        if (article.ShelfLife != command.Article.ShelfLife)
        {
            article.ShelfLife = command.Article.ShelfLife;
        }

        if (article.Color != command.Article.Color)
        {
            article.Color = command.Article.Color;
        }

        if (command.Article.Metrics is { } metrics)
        {
            if (!article.Mass.Equals(metrics.Mass))
            {
                article.Mass = metrics.Mass;
            }

            if (!article.LengthX.Equals(metrics.LengthX))
            {
                article.LengthX = metrics.LengthX;
            }

            if (!article.LengthY.Equals(metrics.LengthY))
            {
                article.LengthY = metrics.LengthY;
            }

            if (!article.LengthZ.Equals(metrics.LengthZ))
            {
                article.LengthZ = metrics.LengthZ;
            }
        }

        // TODO: Move this to inside a ArticleBarcodesDto to only edit the article barcodes if the dto is not null because the way it is can have some problems when more than one client tries to edit the entity at the same time.
        await articleService.SetEan13(article, command.Article.Ean13, cancellationToken);

        await articleService.SetEan8(article, command.Article.Ean8, cancellationToken);

        await articleService.SetUpcA(article, command.Article.UpcA, cancellationToken);

        await articleService.SetUpcE(article, command.Article.UpcE, cancellationToken);

        await articleService.SetCode128(article, command.Article.Code128, cancellationToken);

        await articleService.SetCode39(article, command.Article.Code39, cancellationToken);

        await articleService.SetItf14(article, command.Article.Itf14, cancellationToken);

        await articleService.SetGs1128(article, command.Article.Gs1128, cancellationToken);

        await articleService.SetQrCode(article, command.Article.QrCode, cancellationToken);

        await articleService.SetDataMatrix(article, command.Article.DataMatrix, cancellationToken);

        if (command.Article.Partitions is { } requestedPartitions)
        {
            foreach (var partitionGuid in requestedPartitions)
            {
                if (article.Partitions.Any(p => p.Guid == partitionGuid))
                {
                    continue;
                }

                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                actor.ValidateHasPartitionAccess(partition.Guid);

                article.Partitions.Add(partition);
            }

            var partitionsToRemove = article.Partitions
                .Where(p => !requestedPartitions.Contains(p.Guid))
                .ToList();

            foreach (var partition in partitionsToRemove)
            {
                actor.ValidateHasPartitionAccess(partition.Guid);

                article.Partitions.Remove(partition);
            }
        }

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article update flow completed for article {ArticleGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}.",
                article.Guid,
                actor.ActorGuid,
                article.Partitions.Count);
        }
    }
}

#endregion Update
