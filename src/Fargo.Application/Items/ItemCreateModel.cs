namespace Fargo.Application.Items;

/// <summary>
/// Represents the data required to create a new item.
/// </summary>
/// <param name="ArticleGuid">
/// The unique identifier of the article associated with the item.
/// The item inherits its conceptual definition (e.g., name and description)
/// from the referenced article.
/// </param>
/// <param name="FirstPartition">
/// Optional identifier of the first partition to associate with the item
/// during creation.
/// </param>
/// <remarks>
/// An item represents a concrete instance of an <c>Article</c>.
///
/// Items are partitioned entities and may belong to one or more partitions.
/// When <paramref name="FirstPartition"/> is provided, the created item
/// is initially associated with that partition.
///
/// If no partition is specified, the association behavior depends on
/// the application rules (e.g., default partition assignment or validation failure).
/// </remarks>
public record ItemCreateModel(
        Guid ArticleGuid,
        Guid? FirstPartition = null
        );
