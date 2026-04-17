using Fargo.Domain.Collections;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Entities;

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
    public required Name Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the description of the article.
    /// </summary>
    /// <remarks>
    /// If no description is explicitly provided, the value defaults to
    /// <see cref="Description.Empty"/>.
    /// </remarks>
    public Description Description
    {
        get;
        set;
    } = Description.Empty;

    /// <summary>
    /// Gets the partitions associated with the article.
    /// </summary>
    /// <remarks>
    /// These partitions define the partition scope of the article and are
    /// used in partition-based access evaluation.
    /// </remarks>
    public PartitionCollection Partitions
    {
        get;
        init;
    } = [];

    // TODO: Fix documentation. Is stored in database the unit value and unit type of the mass to dont lose precision
    /// <summary>
    /// Gets or sets the physical mass of the article.
    /// Stored in grams in the database; the original value and unit are preserved while in memory.
    /// </summary>
    public Mass? Mass { get; set; }

    // TODO: Fix documentation. Is stored in database the unit value and unit type of the mass to dont lose precision
    /// <summary>
    /// Gets or sets the X dimension of the article.
    /// Stored in meters in the database; the original value and unit are preserved while in memory.
    /// </summary>
    public Length? LengthX { get; set; }

    // TODO: Fix documentation. Is stored in database the unit value and unit type of the mass to dont lose precision
    /// <summary>
    /// Gets or sets the Y dimension of the article.
    /// Stored in meters in the database; the original value and unit are preserved while in memory.
    /// </summary>
    public Length? LengthY { get; set; }

    // TODO: Fix documentation. Is stored in database the unit value and unit type of the mass to dont lose precision
    /// <summary>
    /// Gets or sets the Z dimension of the article.
    /// Stored in meters in the database; the original value and unit are preserved while in memory.
    /// </summary>
    public Length? LengthZ { get; set; }

    /// <summary>
    /// Gets the barcodes associated with the article.
    /// </summary>
    public BarcodeCollection Barcodes { get; init; } = [];

    // TODO: I think insted of storing a string key, should create a table in the database for that that will contain the image information and how to get the image.
    /// <summary>
    /// Gets or sets the storage key of the article's image.
    /// When <see langword="null"/>, the article has no image.
    /// The format of this key is determined by the configured storage provider,
    /// allowing transparent migration between local disk, S3, or other backends.
    /// </summary>
    public string? ImageKey { get; set; }

    /// <inheritdoc />
    IReadOnlyCollection<IPartitionEntity> IPartitionedEntity.Partitions => Partitions;
}
