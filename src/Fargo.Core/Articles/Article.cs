using Fargo.Core.Entities;
using Fargo.Core.Partitions;
using Fargo.Core.Shared;
using Fargo.Core.Shared.Articles;
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
public class Article : IEntity, IPartitioned
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

    public ArticleBarcode Barcode { get; private init; }

    /// <summary>
    /// Gets the volume of the article.
    /// </summary>
    public Volume? Volume => LengthX * LengthY * LengthZ;

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
    /// Gets a value indicating whether this article is a variation of another article.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Variation))]
    public bool IsVariation => Variation is not null;

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

    public IReadOnlyCollection<ArticleKitComponent>? KitComponents { get; private init; }

    /// <summary>
    /// Gets a value indicating whether this article represents a kit.
    /// </summary>
    [MemberNotNullWhen(true, nameof(KitComponents))]
    public bool IsKit => KitComponents is not null;

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

    /// <summary>
    /// Gets the partitions associated with the article.
    /// </summary>
    /// <remarks>
    /// These partitions define the partition scope of the article and are
    /// used in partition-based access evaluation.
    /// </remarks>
    public IReadOnlyCollection<Partition> Partitions => partitions;

    private readonly List<Partition> partitions = [];

    private Article()
    {
        Barcode = new ArticleBarcode(this);
    }

    private Article(Article variationFromArticle)
    {
        Variation = new ArticleVariation(variationFromArticle, this);
        ArticleType = ArticleType.Variation;
        Barcode = new ArticleBarcode(this);
    }

    private Article(Article packFromArticle, Scalar quantity)
    {
        Pack = new ArticlePack(this, packFromArticle, quantity);
        ArticleType = ArticleType.Pack;
        Barcode = new ArticleBarcode(this);
    }

    private Article(IReadOnlyCollection<(Article, Scalar)> articleKitComponentsValues)
    {
        var kitComponents = new List<ArticleKitComponent>();

        foreach (var component in articleKitComponentsValues)
        {
            kitComponents.Add(new ArticleKitComponent(this, component.Item1, component.Item2));
        }

        KitComponents = kitComponents;

        ArticleType = ArticleType.Kit;
        Barcode = new ArticleBarcode(this);
    }

    private Article(bool isContainer)
    {
        if (isContainer)
        {
            Container = new ArticleContainer(this);
            ArticleType = ArticleType.Container;
        }

        ArticleType = ArticleType.Default;
        Barcode = new ArticleBarcode(this);
    }

    /// <summary>
    /// Creates a new article.
    /// </summary>
    /// <param name="name">The name of the article.</param>
    /// <returns></returns>
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
    /// <returns></returns>
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
    /// <returns></returns>
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
    /// <returns></returns>
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
    /// <returns></returns>
    public static Article NewArticleContainer(Name name)
    {
        var articleContainer = new Article(isContainer: true)
        {
            Name = name,
        };

        return articleContainer;
    }

    public void SetMetrics(Mass? mass, Length? lengthX, Length? lengthY, Length? lengthZ)
    {
        Mass = mass;
        LengthX = lengthX;
        LengthY = lengthY;
        LengthZ = lengthZ;
    }

    public void AddPartition(Partition partition)
    {
        partitions.Add(partition);
    }

    public void RemovePartition(Partition partition)
    {
        partitions.Remove(partition);
    }
}
