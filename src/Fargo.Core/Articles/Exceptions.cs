using Fargo.Core.Barcodes;

namespace Fargo.Core.Articles;

/// <summary>
/// Exception thrown when an attempt is made to delete an article
/// that still has items associated with it.
/// </summary>
public sealed class ArticleDeleteWithItemsAssociatedFargoDomainException(Guid articleGuid)
    : FargoDomainException($"Article '{articleGuid}' cannot be deleted because it has associated items.")
{
    /// <summary>
    /// Gets the identifier of the article that could not be deleted.
    /// </summary>
    public Guid ArticleGuid { get; } = articleGuid;
}

/// <summary>
/// Exception thrown when a barcode is already assigned to a different article.
/// </summary>
public class ArticleBarcodeAlreadyInUseFargoDomainException(BarcodeFormat format, string code)
    : FargoDomainException($"Barcode '{code}' ({format}) is already assigned to another article.")
{
    /// <summary>Gets the barcode format that conflicts.</summary>
    public BarcodeFormat Format { get; } = format;

    /// <summary>Gets the barcode code that conflicts.</summary>
    public string Code { get; } = code;
}

public sealed class ArticleEditNotStartedFargoDomainException(Guid articleGuid)
    : FargoDomainException($"Article '{articleGuid}' cannot be edited before an edit session is started.")
{
    public Guid ArticleGuid { get; } = articleGuid;
}

public sealed class ArticleActionNotAuthorizedFargoDomainException(
    Guid articleGuid,
    Guid actorGuid,
    ActionType action
) : FargoDomainException(
    $"Actor '{actorGuid}' is not authorized to perform action '{action}' on article '{articleGuid}'.")
{
    public Guid ArticleGuid { get; } = articleGuid;

    public Guid ActorGuid { get; } = actorGuid;

    public ActionType Action { get; } = action;
}

public sealed class ArticleAccessDeniedFargoDomainException(
    Guid articleGuid,
    Guid actorGuid
) : FargoDomainException(
    $"Actor '{actorGuid}' does not have access to article '{articleGuid}'.")
{
    public Guid ArticleGuid { get; } = articleGuid;

    public Guid ActorGuid { get; } = actorGuid;
}

public sealed class ArticlePartitionAccessDeniedFargoDomainException(
    Guid articleGuid,
    Guid partitionGuid,
    Guid actorGuid
) : FargoDomainException(
    $"Actor '{actorGuid}' does not have access to partition '{partitionGuid}' for article '{articleGuid}'.")
{
    public Guid ArticleGuid { get; } = articleGuid;

    public Guid PartitionGuid { get; } = partitionGuid;

    public Guid ActorGuid { get; } = actorGuid;
}
