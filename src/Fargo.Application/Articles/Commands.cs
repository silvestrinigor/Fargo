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

        var articleActor = actor.ToActor();

        var article = new Article(command.Article.Name, articleActor);

        if (command.Article.Description is { } description)
        {
            article.ChangeDescription(description);
        }

        article.SetShelfLife(command.Article.ShelfLife);

        article.SetColor(command.Article.Color);

        if (command.Article.Metrics is { } metrics)
        {
            article.SetMetrics(metrics.Mass, metrics.LengthX, metrics.LengthY, metrics.LengthZ);
        }

        await articleService.SetEan13(command.Article.Ean13, article, articleActor, cancellationToken);

        await articleService.SetEan8(command.Article.Ean8, article, articleActor, cancellationToken);

        await articleService.SetUpcA(command.Article.UpcA, article, articleActor, cancellationToken);

        await articleService.SetUpcE(command.Article.UpcE, article, articleActor, cancellationToken);

        await articleService.SetCode128(command.Article.Code128, article, articleActor, cancellationToken);

        await articleService.SetCode39(command.Article.Code39, article, articleActor, cancellationToken);

        await articleService.SetItf14(command.Article.Itf14, article, articleActor, cancellationToken);

        await articleService.SetGs1128(command.Article.Gs1128, article, articleActor, cancellationToken);

        await articleService.SetQrCode(command.Article.QrCode, article, articleActor, cancellationToken);

        await articleService.SetDataMatrix(command.Article.DataMatrix, article, articleActor, cancellationToken);

        foreach (var partitionGuid in command.Article.Partitions ?? [])
        {
            var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

            actor.ValidateHasPartitionAccess(partition.Guid);

            article.AddPartition(partition);
        }

        if (command.Article.IsActive == false)
        {
            article.Deactivate();
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
        var articleActor = actor.ToActor();
        article.StartEdit(articleActor);

        article.Rename(command.Article.Name);
        article.ChangeDescription(command.Article.Description);
        article.SetShelfLife(command.Article.ShelfLife);
        article.SetColor(command.Article.Color);

        if (command.Article.Metrics is { } metrics)
        {
            article.SetMetrics(metrics.Mass, metrics.LengthX, metrics.LengthY, metrics.LengthZ);
        }

        if (command.Article.IsActive)
        {
            article.Activate();
        }
        else
        {
            article.Deactivate();
        }

        // TODO: Move this to inside a ArticleBarcodesDto to only edit the article barcodes if the dto is not null because the way it is can have some problems when more than one client tries to edit the entity at the same time.
        await articleService.SetEan13(command.Article.Ean13, article, articleActor, cancellationToken);

        await articleService.SetEan8(command.Article.Ean8, article, articleActor, cancellationToken);

        await articleService.SetUpcA(command.Article.UpcA, article, articleActor, cancellationToken);

        await articleService.SetUpcE(command.Article.UpcE, article, articleActor, cancellationToken);

        await articleService.SetCode128(command.Article.Code128, article, articleActor, cancellationToken);

        await articleService.SetCode39(command.Article.Code39, article, articleActor, cancellationToken);

        await articleService.SetItf14(command.Article.Itf14, article, articleActor, cancellationToken);

        await articleService.SetGs1128(command.Article.Gs1128, article, articleActor, cancellationToken);

        await articleService.SetQrCode(command.Article.QrCode, article, articleActor, cancellationToken);

        await articleService.SetDataMatrix(command.Article.DataMatrix, article, articleActor, cancellationToken);

        if (command.Article.Partitions is { } requestedPartitions)
        {
            foreach (var partitionGuid in requestedPartitions)
            {
                if (article.Partitions.Any(p => p.Guid == partitionGuid))
                {
                    continue;
                }

                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                article.AddPartition(partition);
            }

            var partitionsToRemove = article.Partitions
                .Where(p => !requestedPartitions.Contains(p.Guid))
                .ToList();

            foreach (var partition in partitionsToRemove)
            {
                article.RemovePartition(partition);
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
