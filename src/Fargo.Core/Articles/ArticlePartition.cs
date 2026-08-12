using Fargo.Core.Partitions;

namespace Fargo.Core.Articles;

/// <summary>
/// Represents an association between an article and a partition.
/// </summary>
/// <remarks>
/// This association defines a partition that is directly assigned to the article
/// and contributes to the article's partition scope.
/// </remarks>
public class ArticlePartition
{
    /// <summary>
    /// Gets the unique identifier of the associated article.
    /// </summary>
    public Guid ArticleGuid { get; private init; }

    /// <summary>
    /// Gets the associated article.
    /// </summary>
    public Article Article { get; private init; }

    /// <summary>
    /// Gets the unique identifier of the associated partition.
    /// </summary>
    public Guid PartitionGuid { get; private init; }

    /// <summary>
    /// Gets the associated partition.
    /// </summary>
    public Partition Partition { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ArticlePartition() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Initializes a new association between the specified article and partition.
    /// </summary>
    /// <param name="article">The article to associate with the partition.</param>
    /// <param name="partition">The partition to associate with the article.</param>
    internal ArticlePartition(Article article, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(article);
        ArgumentNullException.ThrowIfNull(partition);

        Article = article;
        ArticleGuid = article.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }

    /// <summary>
    /// Initializes a new association between the specified article and a partition
    /// identified by its unique identifier.
    /// </summary>
    /// <remarks>
    /// This constructor should be used when the partition entity does not need to be
    /// loaded, such as when assigning a well-known or global partition whose unique
    /// identifier is known and stable.
    /// 
    /// Prefer this constructor over the constructor that accepts a <see cref="Partition"/>
    /// when only the partition identifier is required. This avoids requiring the
    /// partition entity to be loaded solely for the purpose of creating the association.
    /// </remarks>
    /// <param name="article">The article to associate with the partition.</param>
    /// <param name="partitionGuid">
    /// The unique identifier of the partition. This should be a known, valid partition
    /// identifier, such as the identifier of the global partition.
    /// </param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal ArticlePartition(Article article, Guid partitionGuid)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        ArgumentNullException.ThrowIfNull(article);

        if (partitionGuid == Guid.Empty)
        {
            throw new ArgumentException(
                "Partition GUID cannot be empty.",
                nameof(partitionGuid));
        }

        Article = article;
        ArticleGuid = article.Guid;

        PartitionGuid = partitionGuid;
    }
}
