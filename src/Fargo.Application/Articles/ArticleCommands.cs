using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Application.Workspaces;
using Fargo.Core;
using Fargo.Core.Articles;
using Fargo.Core.Barcodes;
using Fargo.Core.Partitions;
using Microsoft.Extensions.Logging;
using System.Drawing;
using UnitsNet;

namespace Fargo.Application.Articles;

#region Create

/// <summary>
/// Command used to create a new article.
/// </summary>
/// <param name="Name">
/// Article name.
/// </param>
public sealed record ArticleCreateCommand(
    Name Name,
    ReservedArticleGuid? ArticleGuid = null
) : ICommand<Guid>;

/// <summary>
/// Handles article creation.
/// </summary>
/// <remarks>
/// Creates the article, validates permissions,
/// and stores the new entity.
/// </remarks>
public sealed class ArticleCreateCommandHandler(
    IArticleRepository articleRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IReservedGuidSession reservedGuidSession,
    ILogger<ArticleCreateCommandHandler> logger
) : ICommandHandler<ArticleCreateCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Article create flow started for actor {ActorGuid}.", actor.Guid);
        }

        var articleGuid = reservedGuidSession.ResolveArticleGuid(command.ArticleGuid);

        var article = Article.CreateArticle(articleGuid, command.Name, actor);

        articleRepository.Add(article);

        entityEventRepository.Add(EntityEvent.EntityCreated<Article>(article, actor.Guid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article create mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }

        return article.Guid;
    }
}

/// <summary>
/// Command used to create an article variation from another article.
/// </summary>
/// <param name="Name">
/// Variation article name.
/// </param>
/// <param name="FromArticleGuid">
/// Unique identifier of the source article.
/// </param>
public sealed record ArticleCreateVariationCommand(
    Name Name,
    Guid FromArticleGuid
) : ICommand<Guid>;

/// <summary>
/// Handles article variation creation.
/// </summary>
/// <remarks>
/// Validates permissions, validates access to the source article,
/// and creates a variation linked to the specified article.
/// </remarks>
public sealed class ArticleCreateVariationCommandHandler(
    IArticleRepository articleRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleCreateVariationCommandHandler> logger
) : ICommandHandler<ArticleCreateVariationCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateVariationCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article variation create flow started from article {FromArticleGuid} by actor {ActorGuid}.",
                command.FromArticleGuid,
                actor.Guid);
        }

        var fromArticle = await articleRepository.GetFoundByGuid(command.FromArticleGuid, cancellationToken);

        var article = Article.CreateArticleVariation(command.Name, fromArticle, actor);

        articleRepository.Add(article);

        entityEventRepository.Add(EntityEvent.EntityCreated<Article>(article, actor.Guid));

        return article.Guid;
    }
}

/// <summary>
/// Command used to create an article pack.
/// </summary>
/// <param name="Name">
/// Pack article name.
/// </param>
/// <param name="FromArticleGuid">
/// Unique identifier of the article contained in the pack.
/// </param>
/// <param name="Quantity">
/// Quantity of articles contained in the pack.
/// </param>
public sealed record ArticleCreatePackCommand(
    Name Name,
    Guid FromArticleGuid,
    Scalar Quantity
) : ICommand<Guid>;

/// <summary>
/// Handles article pack creation.
/// </summary>
/// <remarks>
/// Validates permissions, validates access to the source article,
/// and creates a pack article with the specified quantity.
/// </remarks>
public sealed class ArticleCreatePackCommandHandler(
    IArticleRepository articleRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleCreatePackCommandHandler> logger
) : ICommandHandler<ArticleCreatePackCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreatePackCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article pack create flow started from article {FromArticleGuid} by actor {ActorGuid}.",
                command.FromArticleGuid,
                actor.Guid);
        }

        var fromArticle = await articleRepository.GetFoundByGuid(command.FromArticleGuid, cancellationToken);

        var article = Article.CreateArticlePack(command.Name, fromArticle, command.Quantity, actor);

        articleRepository.Add(article);

        entityEventRepository.Add(EntityEvent.EntityCreated<Article>(article, actor.Guid));

        return article.Guid;
    }
}

/// <summary>
/// Command used to create an article kit.
/// </summary>
/// <param name="Name">
/// Kit article name.
/// </param>
/// <param name="Components">
/// Collection of articles that compose the kit.
/// </param>
public sealed record ArticleCreateKitCommand(
    Name Name,
    IReadOnlyCollection<ArticleKitComponentRequest> Components
) : ICommand<Guid>;

/// <summary>
/// Handles article kit creation.
/// </summary>
/// <remarks>
/// Validates permissions, validates access to all component articles,
/// and creates a kit article composed of multiple articles.
/// </remarks>
public sealed class ArticleCreateKitCommandHandler(
    IArticleRepository articleRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleCreateKitCommandHandler> logger
) : ICommandHandler<ArticleCreateKitCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateKitCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article kit create flow started by actor {ActorGuid}. PackCount: {PackCount}.",
                actor.Guid,
                command.Components.Count);
        }

        var components = new List<ArticleKitComponent>(command.Components.Count);

        foreach (var component in command.Components)
        {
            var fromArticle = await articleRepository.GetFoundByGuid(component.ArticleGuid, cancellationToken);

            components.Add(new ArticleKitComponent(fromArticle, component.Quantity));
        }

        var article = Article.CreateArticleKit(command.Name, components, actor);

        articleRepository.Add(article);

        entityEventRepository.Add(EntityEvent.EntityCreated<Article>(article, actor.Guid));

        return article.Guid;
    }
}

/// <summary>
/// Command used to create a container article.
/// </summary>
/// <param name="Name">
/// Container article name.
/// </param>
public sealed record ArticleCreateContainerCommand(
    Name Name
) : ICommand<Guid>;

/// <summary>
/// Handles container article creation.
/// </summary>
/// <remarks>
/// Validates permissions and creates a container article.
/// </remarks>
public sealed class ArticleCreateContainerCommandHandler(
    IArticleRepository articleRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleCreateContainerCommandHandler> logger
) : ICommandHandler<ArticleCreateContainerCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateContainerCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Article container create flow started by actor {ActorGuid}.", actor.Guid);
        }

        var article = Article.CreateArticleContainer(command.Name, actor);

        articleRepository.Add(article);

        entityEventRepository.Add(EntityEvent.EntityCreated<Article>(article, actor.Guid));

        return article.Guid;
    }
}

#endregion Create

#region Container

/// <summary>
/// Command used to update a container article maximum mass.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="MaxMass">
/// Maximum mass allowed by the container, or <see langword="null"/> when unconstrained.
/// </param>
public sealed record ArticleSetContainerMaxMassCommand(
    Guid ArticleGuid,
    Mass? MaxMass
) : ICommand;

/// <summary>
/// Handles container maximum mass changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates the maximum mass constraint of a container article.
/// </remarks>
public sealed class ArticleSetContainerMaxMassCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleSetContainerMaxMassCommandHandler> logger
) : ICommandHandler<ArticleSetContainerMaxMassCommand>
{
    public async Task Handle(
        ArticleSetContainerMaxMassCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article container max mass mutation started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.Guid);
        }

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        article.ValidateCanEdit(actor);

        if (!article.IsContainer)
        {
            throw new ArticleIsNotContainerFargoDomainException(article.Guid);
        }

        if (Nullable.Equals(article.Container!.MaxMass, command.MaxMass))
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Article container max mass mutation skipped for article {ArticleGuid}; max mass is already requested value.",
                    article.Guid);
            }

            return;
        }

        article.SetContainerMaxMass(command.MaxMass, actor);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article container max mass mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }
    }
}

#endregion Container

#region Name

/// <summary>
/// Command used to rename an article.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="Name">
/// New article name.
/// </param>
public sealed record ArticleRenameCommand(
    Guid ArticleGuid,
    Name Name
) : ICommand;

/// <summary>
/// Handles article rename.
/// </summary>
/// <remarks>
/// Validates permissions and updates the article name.
/// </remarks>
public sealed class ArticleRenameCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleRenameCommandHandler> logger
) : ICommandHandler<ArticleRenameCommand>
{
    public async Task Handle(
        ArticleRenameCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article rename flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.Guid);
        }

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        article.ValidateCanEdit(actor);

        if (article.Name == command.Name)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Article rename flow skipped for article {ArticleGuid}; name is already requested value.",
                    article.Guid);
            }

            return;
        }

        article.Rename(command.Name, actor);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article rename mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }
    }
}

#endregion Name

#region Description

/// <summary>
/// Command used to change the article description.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="Description">
/// New article description.
/// </param>
public sealed record ArticleChangeDescriptionCommand(
    Guid ArticleGuid,
    Description Description
) : ICommand;

/// <summary>
/// Handles article description changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates the description.
/// </remarks>
public sealed class ArticleChangeDescriptionCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleChangeDescriptionCommandHandler> logger
) : ICommandHandler<ArticleChangeDescriptionCommand>
{
    public async Task Handle(
        ArticleChangeDescriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article description change flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.Guid);
        }

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        article.ValidateCanEdit(actor);

        if (article.Description == command.Description)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Article description change flow skipped for article {ArticleGuid}; description is already requested value.",
                    article.Guid);
            }

            return;
        }

        article.ChangeDescription(command.Description, actor);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article description change mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }
    }
}

#endregion Description

#region ShelfLife

/// <summary>
/// Command used to define the article shelf life.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="ShelfLife">
/// Shelf life duration.
/// </param>
public sealed record ArticleSetShelfLifeCommand(
    Guid ArticleGuid,
    TimeSpan? ShelfLife
) : ICommand;

/// <summary>
/// Handles article shelf life changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates shelf life.
/// </remarks>
public sealed class ArticleSetShelfLifeCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleSetShelfLifeCommandHandler> logger
) : ICommandHandler<ArticleSetShelfLifeCommand>
{
    public async Task Handle(
        ArticleSetShelfLifeCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article shelf life mutation started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.Guid);
        }

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        article.ValidateCanEdit(actor);

        if (article.ShelfLife == command.ShelfLife)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Article shelf life mutation skipped for article {ArticleGuid}; shelf life is already requested value.",
                    article.Guid);
            }

            return;
        }

        article.SetShelfLife(command.ShelfLife, actor);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article shelf life mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }
    }
}

#endregion ShelfLife

#region Color

/// <summary>
/// Command used to define the article color.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="Color">
/// Article color.
/// </param>
public sealed record ArticleSetColorCommand(
    Guid ArticleGuid,
    Color? Color
) : ICommand;

/// <summary>
/// Handles article color changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates the color.
/// </remarks>
public sealed class ArticleSetColorCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleSetColorCommandHandler> logger
) : ICommandHandler<ArticleSetColorCommand>
{
    public async Task Handle(
        ArticleSetColorCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article color mutation started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.Guid);
        }

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        article.ValidateCanEdit(actor);

        if (article.Color == command.Color)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Article color mutation skipped for article {ArticleGuid}; color is already requested value.",
                    article.Guid);
            }

            return;
        }

        article.SetColor(command.Color, actor);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article color mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }
    }
}

#endregion Color

#region Activate

/// <summary>
/// Command used to activate an article.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
public sealed record ArticleActivateCommand(
    Guid ArticleGuid
) : ICommand;

/// <summary>
/// Handles article activation.
/// </summary>
/// <remarks>
/// Validates permissions and activates the article.
/// </remarks>
public sealed class ArticleActivateCommandHandler(
    IArticleRepository articleRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleActivateCommandHandler> logger
) : ICommandHandler<ArticleActivateCommand>
{
    public async Task Handle(
        ArticleActivateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article activation flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.Guid);
        }

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        article.ValidateCanEdit(actor);

        if (article.IsActive)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Article activation flow skipped for article {ArticleGuid}; article is already active.",
                    article.Guid);
            }

            return;
        }

        article.Activate(actor);

        entityEventRepository.Add(EntityEvent.Activated<Article>(article, actor.Guid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article activation mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }
    }
}

#endregion Activate

#region Deactivate

/// <summary>
/// Command used to deactivate an article.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
public sealed record ArticleDeactivateCommand(
    Guid ArticleGuid
) : ICommand;

/// <summary>
/// Handles article deactivation.
/// </summary>
/// <remarks>
/// Validates permissions and deactivates the article.
/// </remarks>
public sealed class ArticleDeactivateCommandHandler(
    IArticleRepository articleRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleDeactivateCommandHandler> logger
) : ICommandHandler<ArticleDeactivateCommand>
{
    public async Task Handle(
        ArticleDeactivateCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article deactivation flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.Guid);
        }

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        article.ValidateCanEdit(actor);

        if (!article.IsActive)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Article deactivation flow skipped for article {ArticleGuid}; article is already inactive.",
                    article.Guid);
            }

            return;
        }

        article.Deactivate(actor);

        entityEventRepository.Add(EntityEvent.Deactivated<Article>(article, actor.Guid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article deactivation mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }
    }
}

#endregion Deactivate

#region Metrics

/// <summary>
/// Command used to update article metrics.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="Metrics">
/// Article metrics information.
/// </param>
public sealed record ArticleSetMetricsCommand(
    Guid ArticleGuid,
    ArticleMetrics Metrics
) : ICommand;

/// <summary>
/// Handles article metrics changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates article metrics.
/// </remarks>
public sealed class ArticleSetMetricsCommandHandler(
    IArticleRepository articleRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleSetMetricsCommandHandler> logger
) : ICommandHandler<ArticleSetMetricsCommand>
{
    public async Task Handle(
        ArticleSetMetricsCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article metrics mutation started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.Guid);
        }

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        article.ValidateCanEdit(actor);

        if (Nullable.Equals(article.Mass, command.Metrics.Mass) &&
            Nullable.Equals(article.LengthX, command.Metrics.LengthX) &&
            Nullable.Equals(article.LengthY, command.Metrics.LengthY) &&
            Nullable.Equals(article.LengthZ, command.Metrics.LengthZ))
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Article metrics mutation skipped for article {ArticleGuid}; metrics are already requested values.",
                    article.Guid);
            }

            return;
        }

        article.SetMetrics(command.Metrics, actor);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article metrics mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }
    }
}

#endregion Metrics

#region Barcodes

/// <summary>
/// Command used to update article barcodes.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="Barcodes">
/// Article barcode information.
/// </param>
public sealed record ArticleSetBarcodesCommand(
    Guid ArticleGuid,
    ArticleBarcodesSet Barcodes
) : ICommand;

/// <summary>
/// Handles article barcode changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates article barcodes.
/// </remarks>
public sealed class ArticleSetBarcodesCommandHandler(
    IArticleRepository articleRepository,
    ArticleService articleService,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleSetBarcodesCommandHandler> logger
) : ICommandHandler<ArticleSetBarcodesCommand>
{
    public async Task Handle(
        ArticleSetBarcodesCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article barcode mutation started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.Guid);
        }

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        article.ValidateCanEdit(actor);

        var hasChanges =
            !EqualityComparer<Ean13?>.Default.Equals(article.Ean13, command.Barcodes.Ean13) ||
            !EqualityComparer<Ean8?>.Default.Equals(article.Ean8, command.Barcodes.Ean8) ||
            !EqualityComparer<UpcA?>.Default.Equals(article.UpcA, command.Barcodes.UpcA) ||
            !EqualityComparer<UpcE?>.Default.Equals(article.UpcE, command.Barcodes.UpcE) ||
            !EqualityComparer<Code128?>.Default.Equals(article.Code128, command.Barcodes.Code128) ||
            !EqualityComparer<Code39?>.Default.Equals(article.Code39, command.Barcodes.Code39) ||
            !EqualityComparer<Itf14?>.Default.Equals(article.Itf14, command.Barcodes.Itf14) ||
            !EqualityComparer<Gs1128?>.Default.Equals(article.Gs1128, command.Barcodes.Gs1128) ||
            !EqualityComparer<QrCode?>.Default.Equals(article.QrCode, command.Barcodes.QrCode) ||
            !EqualityComparer<DataMatrix?>.Default.Equals(article.DataMatrix, command.Barcodes.DataMatrix);

        if (!hasChanges)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Article barcode mutation skipped for article {ArticleGuid}; barcodes are already requested values.",
                    article.Guid);
            }

            return;
        }

        await articleService.SetEan13(command.Barcodes.Ean13, article, actor, cancellationToken);

        await articleService.SetEan8(command.Barcodes.Ean8, article, actor, cancellationToken);

        await articleService.SetUpcA(command.Barcodes.UpcA, article, actor, cancellationToken);

        await articleService.SetUpcE(command.Barcodes.UpcE, article, actor, cancellationToken);

        await articleService.SetCode128(command.Barcodes.Code128, article, actor, cancellationToken);

        await articleService.SetCode39(command.Barcodes.Code39, article, actor, cancellationToken);

        await articleService.SetItf14(command.Barcodes.Itf14, article, actor, cancellationToken);

        await articleService.SetGs1128(command.Barcodes.Gs1128, article, actor, cancellationToken);

        await articleService.SetQrCode(command.Barcodes.QrCode, article, actor, cancellationToken);

        await articleService.SetDataMatrix(command.Barcodes.DataMatrix, article, actor, cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article barcode mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }
    }
}

#endregion Barcodes

#region Partitions

/// <summary>
/// Command used to update article partitions.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
/// <param name="PartitionGuids">
/// Collection of partition identifiers.
/// </param>
public sealed record ArticleSetPartitionsCommand(
    Guid ArticleGuid,
    IReadOnlyCollection<Guid> PartitionGuids
) : ICommand;

/// <summary>
/// Handles article partition changes.
/// </summary>
/// <remarks>
/// Validates permissions and updates article partitions.
/// </remarks>
public sealed class ArticleSetPartitionsCommandHandler(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    IEntityPartitionEventRepository entityPartitionEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleSetPartitionsCommandHandler> logger
) : ICommandHandler<ArticleSetPartitionsCommand>
{
    public async Task Handle(
        ArticleSetPartitionsCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article partition mutation started for article {ArticleGuid} by actor {ActorGuid}. RequestedPartitionCount: {RequestedPartitionCount}.",
                command.ArticleGuid,
                actor.Guid,
                command.PartitionGuids.Count);
        }

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        article.ValidateCanEdit(actor);

        var requestedPartitionGuids = command.PartitionGuids.Distinct().ToArray();

        var hasChanges =
            article.Partitions.Count != requestedPartitionGuids.Length ||
            article.Partitions.Any(p => !requestedPartitionGuids.Contains(p.Guid));

        if (!hasChanges)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Article partition mutation skipped for article {ArticleGuid}; partitions are already requested values.",
                    article.Guid);
            }

            return;
        }

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

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article partition mutation completed for article {ArticleGuid} by actor {ActorGuid}. PartitionCount: {PartitionCount}.",
                article.Guid,
                actor.Guid,
                article.Partitions.Count);
        }
    }
}

#endregion Partitions

#region Delete

/// <summary>
/// Command used to delete an article.
/// </summary>
/// <param name="ArticleGuid">
/// Article unique identifier.
/// </param>
public sealed record ArticleDeleteCommand(
    Guid ArticleGuid
) : ICommand;

/// <summary>
/// Handles article deletion.
/// </summary>
/// <remarks>
/// Validates permissions and removes the article.
/// </remarks>
public sealed class ArticleDeleteCommandHandler(
    IArticleRepository articleRepository,
    IEntityEventRepository entityEventRepository,
    ICurrentAuthorizationContext currentAuthorizationContext,
    ILogger<ArticleDeleteCommandHandler> logger
) : ICommandHandler<ArticleDeleteCommand>
{
    public async Task Handle(
        ArticleDeleteCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);
        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article delete flow started for article {ArticleGuid} by actor {ActorGuid}.",
                command.ArticleGuid,
                actor.Guid);
        }

        var article = await articleRepository.GetFoundByGuid(command.ArticleGuid, cancellationToken);

        article.ValidateCanDelete(actor);

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

        entityEventRepository.Add(EntityEvent.EntityDeleted<Article>(article, actor.Guid));

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article delete mutation completed for article {ArticleGuid} by actor {ActorGuid}.",
                article.Guid,
                actor.Guid);
        }
    }
}

#endregion Delete
