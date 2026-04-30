using Fargo.Domain.Barcodes;
using Fargo.Domain.Partitions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fargo.Domain.Articles;

/// <summary>
/// Represents an article in the system.
/// </summary>
/// <remarks>
/// An article defines the descriptive information of a product or item type,
/// such as its name and description. It does not represent a physical unit,
/// but rather the conceptual definition shared by one or more items.
///
/// An article is partitioned data and may belong to multiple
/// <see cref="Partition"/> instances. A user may access the article only
/// if they have access to at least one of its partitions, subject to any
/// additional authorization rules.
/// </remarks>
public class Article : ModifiedEntity, IPartitionedEntity
{
    /// <summary>
    /// Gets or sets the name of the article.
    /// </summary>
    /// <remarks>
    /// The name identifies the article in the domain and must satisfy
    /// the validation rules defined by <see cref="Name"/>.
    /// </remarks>
    public required Name Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the article.
    /// </summary>
    /// <remarks>
    /// If no description is explicitly provided, the value defaults to
    /// <see cref="Description.Empty"/>.
    /// </remarks>
    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// Gets or sets the physical measurements of the article, including mass, dimensions, and
    /// computed density.
    /// </summary>
    public ArticleMetrics Metrics { get; set; } = new();

    /// <summary>
    /// Gets or sets the shelf life of the article.
    /// When <see langword="null"/>, no shelf life constraint is defined.
    /// Persisted as <c>bigint</c> (ticks) in the database.
    /// </summary>
    public TimeSpan? ShelfLife { get; set; }

    /// <summary>
    /// Gets or sets the image metadata for the article.
    /// </summary>
    public ArticleImages Images { get; set; } = new();

    /// <summary>
    /// Gets or sets the storage key of the article's image.
    /// </summary>
    [NotMapped]
    [Obsolete("Use Images.ImageKey instead.")]
    public string? ImageKey
    {
        get => Images.ImageKey;
        set => Images.ImageKey = value;
    }

    /// <summary>
    /// Gets the barcodes associated with the article.
    /// </summary>
    public BarcodeCollection Barcodes { get; init; } = [];

    /// <summary>
    /// Gets the partitions associated with the article.
    /// </summary>
    /// <remarks>
    /// These partitions define the partition scope of the article and are
    /// used in partition-based access evaluation.
    /// </remarks>
    public PartitionCollection Partitions { get; init; } = [];

    /// <inheritdoc />
    IReadOnlyCollection<IPartitionEntity> IPartitionedEntity.Partitions => Partitions;
}
