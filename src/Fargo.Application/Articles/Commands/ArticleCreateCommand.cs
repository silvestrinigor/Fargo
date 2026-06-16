using Fargo.Application.Articles;
using Fargo.Application.Identity;
using Fargo.Application.Partitions;
using Fargo.Core.Articles;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Articles;
using Fargo.Core.Shared.Barcodes;
using Microsoft.Extensions.Logging;
using System.Drawing;
using UnitsNet;

namespace Fargo.Application.Articles.Commands;

/// <summary>
/// Command used to create a article.
/// </summary>
public sealed record ArticleCreateDefaultCommand(
    Name Name,
    Description? Description = null,
    ArticleType? ArticleType = null,
    TimeSpan? ShelfLife = null,
    Color? Color = null,
    Mass? Mass = null,
    Length? LengthX = null,
    Length? LengthY = null,
    Length? LengthZ = null,
    Ean13? Ean13 = null,
    Ean8? Ean8 = null,
    UpcA? UpcA = null,
    UpcE? UpcE = null,
    Code128? Code128 = null,
    Code39? Code39 = null,
    Itf14? Itf14 = null,
    Gs1128? Gs1128 = null,
    QrCode? QrCode = null,
    DataMatrix? DataMatrix = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    bool? IsActive = null
) : ICommand<Guid>;

/// <summary>
/// Handles article creation.
/// </summary>
public sealed class ArticleCreateDefaultCommandHandler(
    IArticleRepository articleRepository,
    IPartitionRepository partitionRepository,
    ArticleService articleService,
    ICurrentAuthorizationContext currentAuthorizationContext,
    IUnitOfWork unitOfWork,
    ILogger<ArticleCreateDefaultCommandHandler> logger
) : ICommandHandler<ArticleCreateDefaultCommand, Guid>
{
    public async Task<Guid> Handle(
        ArticleCreateDefaultCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorizationContext = await currentAuthorizationContext.GetAsync(cancellationToken);

        var actor = authorizationContext.ToActor();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article create flow started for actor {actorId}.",
                actor.ActorId);
        }

        actor.ThrowIfPermissionNotAuthorized(ActionType.CreateArticle);

        var article = Article.NewArticle(command.Name);

        article.Description = command.Description ?? Description.Empty;

        article.ShelfLife = command.ShelfLife;

        article.Color = command.Color;

        article.SetMetrics(
            command.Mass,
            command.LengthX,
            command.LengthY,
            command.LengthZ
        );

        await articleService.SetEan13(command.Ean13, article, actor, cancellationToken);

        await articleService.SetEan8(command.Ean8, article, actor, cancellationToken);

        await articleService.SetUpcA(command.UpcA, article, actor, cancellationToken);

        await articleService.SetUpcE(command.UpcE, article, actor, cancellationToken);

        await articleService.SetCode128(barcodes.Code128, article, actor, cancellationToken);

        await articleService.SetCode39(barcodes.Code39, article, actor, cancellationToken);

        await articleService.SetItf14(barcodes.Itf14, article, actor, cancellationToken);

        await articleService.SetGs1128(barcodes.Gs1128, article, actor, cancellationToken);

        await articleService.SetQrCode(barcodes.QrCode, article, actor, cancellationToken);

        await articleService.SetDataMatrix(barcodes.DataMatrix, article, actor, cancellationToken);

        if (command.Partitions is { Count: > 0 } partitions)
        {
            foreach (var partitionGuid in partitions.Distinct())
            {
                var partition = await partitionRepository.GetFoundByGuid(partitionGuid, cancellationToken);

                actor.ThrowIfAccessNotAuthorized(partition);

                article.AddPartition(partition);
            }
        }

        article.IsActive = command.IsActive ?? true;

        articleRepository.Add(article);

        await unitOfWork.SaveChanges(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Article create mutation completed for article {articleGuid} by actor {actorId}.",
                article.Guid,
                actor.ActorId);
        }

        return article.Guid;
    }
}
