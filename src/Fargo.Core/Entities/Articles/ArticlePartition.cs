using Fargo.Core.Entities.Partitions;

namespace Fargo.Core.Entities.Articles;

/// <summary>
/// A partition directly assigned to an article; it contributes to the article's partition scope.
/// </summary>
public class ArticlePartition
{
    public Guid ArticleGuid { get; private init; }

    public Article Article { get; private init; }

    public Guid PartitionGuid { get; private init; }

    public Partition Partition { get; private init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ArticlePartition() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal ArticlePartition(Article article, Partition partition)
    {
        ArgumentNullException.ThrowIfNull(article);
        ArgumentNullException.ThrowIfNull(partition);

        Article = article;
        ArticleGuid = article.Guid;

        Partition = partition;
        PartitionGuid = partition.Guid;
    }

    // Prefer this overload when only the identifier is known (for example the global partition),
    // so the partition entity does not have to be loaded just to create the association.
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
