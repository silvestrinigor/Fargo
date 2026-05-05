using Fargo.Domain.Barcodes;
using Fargo.Domain.Partitions;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

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
public class Article : ModifiedEntity, IPartitionedEntity, IActivable
{
    public Article()
    {

    }

    public Article(ArticleVariation variation)
    {
        Variation = variation;
    }

    public Article(ArticlePack pack)
    {
        Pack = pack;
    }

    public Article(ArticleKit kit)
    {
        Kit = kit;
    }

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
    /// Gets or sets the shelf life of the article.
    /// When <see langword="null"/>, no shelf life constraint is defined.
    /// Persisted as <c>bigint</c> (ticks) in the database.
    /// </summary>
    public TimeSpan? ShelfLife { get; set; }

    /// <summary>
    /// Gets or sets the physical measurements of the article, including mass, dimensions, and
    /// computed density.
    /// </summary>
    public ArticleMetrics Metrics { get; set; } = new();

    /// <summary>
    /// Gets or sets the barcodes associated with the article.
    /// </summary>
    public ArticleBarcodes Barcodes { get; set; } = new();

    # region Relation

    /// <summary>
    /// Gets the variation info associated with the article.
    /// When <see langword="null"/>, no variation constraint is defined.
    /// </summary>
    public ArticleVariation? Variation { get; private init; }

    public ArticleContainer? Container { get; private init; }

    public bool IsContainer => Container is not null;

    /// <summary>
    /// Gets the pack info associated with the article.
    /// When <see langword="null"/>, no pack constraint is defined.
    /// </summary>
    public ArticlePack? Pack { get; private init; }

    /// <summary>
    /// Gets the kit info associated with the article.
    /// When <see langword="null"/>, no kit constraint is defined.
    /// </summary>
    public ArticleKit? Kit { get; private init; }

    #endregion Relation

    #region Active

    /// <summary>
    /// Gets a value indicating whether the article is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Activates the article.
    /// </summary>
    public void Activate() => IsActive = true;

    /// <summary>
    /// Deactivates the article.
    /// </summary>
    public void Deactivate() => IsActive = false;

    #endregion Active

    #region Partition

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

    #endregion Partition
}

/// <summary>
/// Groups the physical measurement properties of an <see cref="Article"/>.
/// </summary>
public sealed class ArticleMetrics
{
    #region Length

    /// <summary>
    /// Gets or sets the X dimension of the article.
    /// </summary>
    public Length? LengthX { get; set; }

    /// <summary>
    /// Gets or sets the Y dimension of the article.
    /// </summary>
    public Length? LengthY { get; set; }

    /// <summary>
    /// Gets or sets the Z dimension of the article.
    /// </summary>
    public Length? LengthZ { get; set; }

    #endregion Length

    /// <summary>
    /// Gets or sets the physical mass of the article.
    /// </summary>
    public Mass? Mass { get; set; }

    /// <summary>
    /// Gets the volume of the article.
    /// </summary>
    public Volume? Volume => LengthX * LengthY * LengthZ;

    /// <summary>
    /// Gets the density of the article.
    /// </summary>
    public Density? Density => Mass / Volume;
}

/// <summary>
/// Domain facade for an article's barcode collection, grouped by barcode format.
/// Properties are <see langword="null"/> when the article has no barcode in that format.
/// </summary>
public sealed class ArticleBarcodes
{
    /// <summary>
    /// Initializes an empty detached barcode group.
    /// </summary>
    public ArticleBarcodes()
    {
    }

    /// <summary>
    /// EAN-13 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Ean13? Ean13 { get; set; }

    /// <summary>
    /// EAN-8 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Ean8? Ean8 { get; set; }

    /// <summary>
    /// UPC-A barcode, or <see langword="null"/> when absent.
    /// </summary>
    public UpcA? UpcA { get; set; }

    /// <summary>
    /// UPC-E barcode, or <see langword="null"/> when absent.
    /// </summary>
    public UpcE? UpcE { get; set; }

    /// <summary>
    /// Code 128 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Code128? Code128 { get; set; }

    /// <summary>
    /// Code 39 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Code39? Code39 { get; set; }

    /// <summary>
    /// ITF-14 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Itf14? Itf14 { get; set; }

    /// <summary>
    /// GS1-128 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Gs1128? Gs1128 { get; set; }

    /// <summary>
    /// QR Code barcode, or <see langword="null"/> when absent.
    /// </summary>
    public QrCode? QrCode { get; set; }

    /// <summary>
    /// Data Matrix barcode, or <see langword="null"/> when absent.
    /// </summary>
    public DataMatrix? DataMatrix { get; set; }

    /// <summary>
    /// Gets whether this article has no barcodes in any supported format.
    /// </summary>
    public bool IsEmpty =>
        Ean13 is null && Ean8 is null && UpcA is null && UpcE is null &&
        Code128 is null && Code39 is null && Itf14 is null && Gs1128 is null &&
        QrCode is null && DataMatrix is null;
}

public sealed class ArticleContainer
{
    public IReadOnlyCollection<Article>? RestrictList { get; set; }

    public IReadOnlyCollection<Article>? AllowedList { get; set; }

    public Mass? MaxMass { get; set; }
}

public sealed class ArticleVariation
{
    public Guid FromArticleGuid { get; private set; }

    public required Article FromArticle
    {
        get;
        init
        {
            FromArticleGuid = value.Guid;
            field = value;
        }
    }
}

public sealed class ArticlePack
{
    public Guid FromArticleGuid { get; private set; }

    public required Article FromArticle
    {
        get;
        init
        {
            FromArticleGuid = value.Guid;
            field = value;
        }
    }

    public Scalar Quantity { get; set; } = 1.Amount();
}

public sealed class ArticleKit
{
    public required IReadOnlyCollection<ArticlePack> FromArticles { get; init; }
}