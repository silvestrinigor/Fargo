using Fargo.Core.Activables;
using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Articles;
using Fargo.Core.Shared.Barcodes;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using UnitsNet;

namespace Fargo.Core.Articles;

/// <summary>
/// Represents an article in the system.
/// </summary>
/// <remarks>
/// An article defines the descriptive information of a product or item type,
/// such as its name and description. It does not represent a physical unit,
/// but rather the conceptual definition shared by one or more items.
/// </remarks>
public class Article : Entity, IEntityTyped, IActivable, IPartitioned
{
    /// <summary>
    /// Gets or sets the name of the article.
    /// </summary>
    public Name Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the article.
    /// </summary>
    /// <remarks>
    /// If no description is explicitly provided, the value defaults to
    /// <see cref="Description.Empty"/>.
    /// </remarks>
    public Description Description { get; set; } = Description.Empty;

    /// <summary>
    /// Gets the type of the article.
    /// </summary>
    public ArticleType ArticleType { get; }

    /// <summary>
    /// Gets or sets the shelf life of the article.
    /// When <see langword="null"/>, no shelf life constraint is defined.
    /// </summary>
    public TimeSpan? ShelfLife { get; set; }

    /// <summary>
    /// Gets or sets the color of the article.
    /// When <see langword="null"/>, no color constraint is defined.
    /// </summary>
    public Color? Color { get; set; }

    /// <summary>
    /// Gets or sets the value indicating whether the article is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public EntityType GetEntityType() => EntityType.Article;

    /// <summary>
    /// Gets the X dimension of the article.
    /// </summary>
    public Length? LengthX { get; private set; }

    /// <summary>
    /// Gets the Y dimension of the article.
    /// </summary>
    public Length? LengthY { get; private set; }

    /// <summary>
    /// Gets the Z dimension of the article.
    /// </summary>
    public Length? LengthZ { get; private set; }

    /// <summary>
    /// Gets the physical mass of the article.
    /// </summary>
    public Mass? Mass { get; private set; }

    /// <summary>
    /// Gets the volume of the article.
    /// </summary>
    public Volume? Volume => LengthX * LengthY * LengthZ;

    /// <summary>
    /// Gets the density of the article.
    /// </summary>
    public Density? Density => Mass / Volume;

    public void SetMetrics(Mass? mass, Length? lengthX, Length? lengthY, Length? lengthZ)
    {
        Mass = mass;
        LengthX = lengthX;
        LengthY = lengthY;
        LengthZ = lengthZ;
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

    #region Variation

    /// <summary>
    /// Gets the variation info associated with the article.
    /// When <see langword="null"/>, no variation constraint is defined.
    /// </summary>
    public ArticleVariation? Variation { get; private init; }

    /// <summary>
    /// Gets a value indicating whether this article is a variation of another article.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Variation))]
    public bool IsVariation => Variation is not null;

    #endregion

    #region Pack

    /// <summary>
    /// Gets the pack info associated with the article.
    /// When <see langword="null"/>, no pack constraint is defined.
    /// </summary>
    public ArticlePack? Pack { get; private init; }

    /// <summary>
    /// Gets a value indicating whether this article represents a pack.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Pack))]
    public bool IsPack => Pack is not null;

    #endregion Pack

    #region Kit

    /// <summary>
    /// Gets the kit info associated with the article.
    /// When <see langword="null"/>, no kit constraint is defined.
    /// </summary>
    public ArticleKit? Kit { get; private init; }

    /// <summary>
    /// Gets a value indicating whether this article represents a kit.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Kit))]
    public bool IsKit => Kit is not null;

    #endregion Kit

    #region Container

    /// <summary>
    /// Gets the container constraints associated with the article.
    /// When <see langword="null"/>, the article is not a container.
    /// </summary>
    public ArticleContainer? Container { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this article represents a container.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Container))]
    public bool IsContainer => Container is not null;

    #endregion Container

    #region Partition

    /// <summary>
    /// Gets the partitions associated with the article.
    /// </summary>
    /// <remarks>
    /// These partitions define the partition scope of the article and are
    /// used in partition-based access evaluation.
    /// </remarks>
    public IReadOnlyCollection<Partition> Partitions => partitions;

    private readonly List<Partition> partitions = [];

    public void AddPartition(Partition partition)
    {
        if (partitions.Any(p => p.Guid == partition.Guid))
        {
            return;
        }

        partitions.Add(partition);
    }

    public void RemovePartition(Partition partition)
    {
        if (!partitions.Any(p => p.Guid == partition.Guid))
        {
            return;
        }

        partitions.Remove(partition);
    }

    /// <inheritdoc />
    IReadOnlyCollection<IPartition> IPartitioned.Partitions => Partitions;

    #endregion

    // EF
    private Article()
    {
    }

    internal Article(Name name)
    {
        Name = name;
        ArticleType = ArticleType.Default;
    }

    internal Article(Name name, ArticleVariation variation)
        : this(name)
    {
        Variation = variation;
        ArticleType = ArticleType.Variation;
    }

    internal Article(Name name, ArticlePack pack)
        : this(name)
    {
        Pack = pack;
        ArticleType = ArticleType.Pack;
    }

    internal Article(Name name, ArticleKit kit)
        : this(name)
    {
        Kit = kit;
        ArticleType = ArticleType.Kit;
    }

    internal Article(Name name, ArticleContainer container)
        : this(name)
    {
        Container = container;
        ArticleType = ArticleType.Container;
    }

    /// <summary>
    /// Creates a new article.
    /// </summary>
    /// <param name="name">The name of the article.</param>
    /// <returns></returns>
    public static Article NewArticle(Name name)
    {
        return new Article(name);
    }

    /// <summary>
    /// Creates a new article variation.
    /// </summary>
    /// <param name="name">The name of the article.</param>
    /// <param name="fromArticle">The article this article is a variation of.</param>
    /// <returns></returns>
    public static Article NewArticleVariation(Name name, Article fromArticle)
    {
        return new Article(name, new ArticleVariation(fromArticle));
    }

    /// <summary>
    /// Creates a new article pack.
    /// </summary>
    /// <param name="name">The name of the article.</param>
    /// <param name="fromArticle">The article this article is a pack of.</param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    public static Article NewArticlePack(Name name, Article fromArticle, Scalar quantity)
    {
        return new Article(name, new ArticlePack(fromArticle, quantity));
    }

    /// <summary>
    /// creates a new article kit.
    /// </summary>
    /// <param name="name">The name of the article.</param>
    /// <param name="kitComponents"></param>
    /// <returns></returns>
    public static Article NewArticleKit(Name name, IReadOnlyCollection<ArticleKitComponent> kitComponents)
    {
        return new Article(name, new ArticleKit(kitComponents));
    }

    /// <summary>
    /// Creates a new article container.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Article NewArticleContainer(Name name)
    {
        return new Article(name, new ArticleContainer(null));
    }
}
