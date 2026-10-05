using Fargo.Core.Common;
using Fargo.Core.Entities;
using Fargo.Core.Informations;
using Fargo.Core.Partitions;
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
///
/// Every article is always associated with the global partition. Additional
/// partitions may be associated with the article to define its partition scope.
/// </remarks>
public class Article : IEntity, IEntityTyped, IPartitionedGuidsReadOnly
{
    /// <summary>
    /// Gets the unique identifier of the article.
    /// </summary>
    public Guid Guid { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the article.
    /// </summary>
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
    /// Gets the type of the article.
    /// </summary>
    public ArticleType ArticleType { get; private set; }

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
    /// Gets the collection of barcodes associated with the article.
    /// </summary>
    public ArticleBarcode Barcode { get; private init; }

    /// <summary>
    /// Gets the physical dimensions of the article.
    /// </summary>
    public ArticleDimension Dimension { get; private init; }

    /// <summary>
    /// Gets the physical mass of the article.
    /// </summary>
    public Mass? Mass { get; private set; }

    /// <summary>
    /// Gets the volume of the article.
    /// </summary>
    public Volume? Volume => Dimension.X * Dimension.Y * Dimension.Z;

    /// <summary>
    /// Gets the density of the article.
    /// </summary>
    public Density? Density => Mass / Volume;

    /// <summary>
    /// Gets the variation info associated with the article.
    /// When <see langword="null"/>, no variation constraint is defined.
    /// </summary>
    public ArticleVariation? Variation { get; private init; }

    /// <summary>
    /// Gets the pack info associated with the article.
    /// When <see langword="null"/>, no pack constraint is defined.
    /// </summary>
    public ArticlePack? Pack { get; private init; }

    /// <summary>
    /// Gets the components that compose the article kit.
    /// </summary>
    public IReadOnlyCollection<ArticleKitComponent> KitComponents => kitComponents;

    private readonly List<ArticleKitComponent> kitComponents = [];

    /// <summary>
    /// Gets the container constraints associated with the article.
    /// When <see langword="null"/>, the article is not a container.
    /// </summary>
    public ArticleContainer? Container { get; private set; }

    /// <summary>
    /// Gets the partitions associated with the article.
    /// </summary>
    /// <remarks>
    /// Every article is always associated with the global partition.
    /// The global partition defines the base partition scope of the article
    /// and cannot be removed.
    ///
    /// Additional partitions may be associated with the article.
    /// </remarks>
    public IReadOnlyCollection<ArticlePartition> Partitions => partitions;

    /// <summary>
    /// Gets the unique identifiers of the partitions associated with the article.
    /// </summary>
    public IReadOnlyCollection<Guid> PartitionGuids => [.. partitions.Select(p => p.PartitionGuid)];

    private readonly List<ArticlePartition> partitions = [];

    private Article()
    {
        Barcode = new ArticleBarcode(this);
        Dimension = new ArticleDimension(this);

        // Every article must belong to the global partition.
        partitions.Add(new ArticlePartition(this, FargoCoreWellKnowGuids.GlobalPartitionGuid));
    }

    private Article(Article variationFromArticle)
    : this()
    {
        Variation = new ArticleVariation(variationFromArticle, this);
        ArticleType = ArticleType.Variation;
    }

    private Article(Article packFromArticle, Scalar quantity)
    : this()
    {
        Pack = new ArticlePack(this, packFromArticle, quantity);
        ArticleType = ArticleType.Pack;
    }

    private Article(IReadOnlyCollection<(Article, Scalar)> components)
    : this()
    {
        foreach (var (article, quantity) in components)
        {
            kitComponents.Add(new ArticleKitComponent(this, article, quantity));
        }

        ArticleType = ArticleType.Kit;
    }

    private Article(bool isContainer)
    : this()
    {
        if (isContainer)
        {
            Container = new ArticleContainer(this);
            ArticleType = ArticleType.Container;
        }
        else
        {
            ArticleType = ArticleType.Default;
        }
    }

    /// <summary>
    /// Creates a new article.
    /// </summary>
    /// <param name="name">The name of the article.</param>
    /// <returns>
    /// A new <see cref="Article"/> instance.
    /// </returns>
    public static Article NewArticle(Name name)
    {
        var article = new Article
        {
            Name = name,
            ArticleType = ArticleType.Default
        };

        return article;
    }

    /// <summary>
    /// Creates a new article variation.
    /// </summary>
    /// <param name="name">The name of the article.</param>
    /// <param name="fromArticle">The article this article is a variation of.</param>
    /// <returns>
    /// A new <see cref="Article"/> instance.
    /// </returns>
    public static Article NewArticleVariation(Name name, Article fromArticle)
    {
        var articleVariation = new Article(fromArticle)
        {
            Name = name,
        };

        return articleVariation;
    }

    /// <summary>
    /// Creates a new article pack.
    /// </summary>
    /// <param name="name">The name of the article.</param>
    /// <param name="fromArticle">The article this article is a pack of.</param>
    /// <param name="quantity"></param>
    /// <returns>
    /// A new <see cref="Article"/> instance.
    /// </returns>
    public static Article NewArticlePack(Name name, Article fromArticle, Scalar quantity)
    {
        var articlePack = new Article(fromArticle, quantity)
        {
            Name = name,
        };

        return articlePack;
    }

    /// <summary>
    /// creates a new article kit.
    /// </summary>
    /// <param name="name">The name of the article.</param>
    /// <param name="kitComponents"></param>
    /// <returns>
    /// A new <see cref="Article"/> instance.
    /// </returns>
    public static Article NewArticleKit(Name name, IReadOnlyCollection<(Article, Scalar)> kitComponents)
    {
        var articleKit = new Article(kitComponents)
        {
            Name = name
        };

        return articleKit;
    }

    /// <summary>
    /// Creates a new article container.
    /// </summary>
    /// <param name="name"></param>
    /// <returns>
    /// A new <see cref="Article"/> instance.
    /// </returns>
    public static Article NewArticleContainer(Name name)
    {
        var articleContainer = new Article(isContainer: true)
        {
            Name = name,
        };

        return articleContainer;
    }

    /// <summary>
    /// Sets the physical measurements of the article.
    /// </summary>
    /// <param name="mass">
    /// The physical mass of the article.
    /// </param>
    /// <param name="lengthX">
    /// The length along the X axis.
    /// </param>
    /// <param name="lengthY">
    /// The length along the Y axis.
    /// </param>
    /// <param name="lengthZ">
    /// The length along the Z axis.
    /// </param>
    public void SetMetrics(Mass? mass, Length? lengthX, Length? lengthY, Length? lengthZ)
    {
        Mass = mass;
        Dimension.SetDimensions(lengthX, lengthY, lengthZ);
    }

    /// <summary>
    /// Associates the article with the specified partition.
    /// If the association already exists, no action is taken.
    /// </summary>
    /// <param name="partition">The partition to associate.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="partition"/> is <see langword="null"/>.
    /// </exception>
    public void AddPartition(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (partitions.Any(p => p.PartitionGuid == partition.Guid))
        {
            return;
        }

        partitions.Add(new ArticlePartition(this, partition));
    }

    /// <summary>
    /// Removes the association between the article and the specified partition.
    /// </summary>
    /// <remarks>
    /// The global partition is mandatory for every article and therefore
    /// cannot be removed.
    ///
    /// If the article is not associated with the specified partition,
    /// no action is taken.
    /// </remarks>
    /// <param name="partitionGuid">
    /// The identifier of the partition to remove.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to remove the global partition.
    /// </exception>
    public void RemovePartition(Guid partitionGuid)
    {
        if (partitionGuid == FargoCoreWellKnowGuids.GlobalPartitionGuid)
        {
            throw new FargoCoreException(
                $"The global partition '{FargoCoreWellKnowGuids.GlobalPartitionGuid}' is mandatory and cannot be removed from an article.",
                FargoErrorType.InvalidOperation);
        }

        partitions.RemoveAll(p => p.PartitionGuid == partitionGuid);
    }

    /// <inheritdoc/>
    public EntityType GetEntityType() => EntityType.Article;
}
