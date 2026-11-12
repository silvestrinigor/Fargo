using Fargo.Core.Common;
using Fargo.Core.Entities.Partitions;
using Fargo.Core.Informations;
using System.Drawing;
using UnitsNet;

namespace Fargo.Core.Entities.Articles;

/// <summary>
/// The definition of a product type, shared by its items; not a physical unit.
/// </summary>
/// <remarks>
/// Every article belongs to the global partition, which cannot be removed.
/// </remarks>
public class Article : Entity, IPartitionedGuidsReadOnly, IConcurrencyVersioned
{
    public required Name Name { get; set; }

    public Description Description { get; set; } = Description.Empty;

    public ArticleType ArticleType { get; private init; } = ArticleType.Default;

    /// <summary>
    /// Whether stock is tracked as individual items (<see cref="ArticleTrackingMode.Entity"/>)
    /// or as quantities recorded by movement events (<see cref="ArticleTrackingMode.Event"/>).
    /// </summary>
    public ArticleTrackingMode TrackingMode { get; private init; } = ArticleTrackingMode.Entity;

    public TimeSpan? ShelfLife { get; set; }

    public Color? Color { get; set; }

    public ArticleBarcode Barcode { get; private init; }

    public ArticleDimension Dimension { get; private init; }

    public Mass? Mass { get; private set; }

    /// <summary>
    /// <see langword="null"/> unless all three dimensions are set.
    /// </summary>
    public Volume? Volume => Dimension.X * Dimension.Y * Dimension.Z;

    /// <summary>
    /// <see langword="null"/> unless both the mass and the volume are known.
    /// </summary>
    public Density? Density => Mass / Volume;

    public ArticleVariation? Variation { get; private init; }

    public ArticlePack? Pack { get; private init; }

    public IReadOnlyCollection<ArticleKitComponent> KitComponents => kitComponents;

    private readonly List<ArticleKitComponent> kitComponents = [];

    public ArticleContainer? Container { get; private set; }

    public IReadOnlyCollection<ArticlePartition> Partitions => partitions;

    public IReadOnlyCollection<Guid> PartitionGuids => [.. partitions.Select(p => p.PartitionGuid)];

    private readonly List<ArticlePartition> partitions = [];

    private Article()
    {
        EntityType = EntityType.Article;

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

    public static Article NewArticle(Name name, ArticleTrackingMode trackingMode = ArticleTrackingMode.Entity)
    {
        var article = new Article
        {
            Name = name,
            ArticleType = ArticleType.Default,
            TrackingMode = trackingMode,
        };

        return article;
    }

    public static Article NewArticleVariation(Name name, Article fromArticle, ArticleTrackingMode trackingMode = ArticleTrackingMode.Entity)
    {
        var articleVariation = new Article(fromArticle)
        {
            Name = name,
            TrackingMode = trackingMode
        };

        return articleVariation;
    }

    public static Article NewArticlePack(Name name, Article fromArticle, Scalar quantity, ArticleTrackingMode trackingMode = ArticleTrackingMode.Entity)
    {
        var articlePack = new Article(fromArticle, quantity)
        {
            Name = name,
            TrackingMode = trackingMode,
        };

        return articlePack;
    }

    public static Article NewArticleKit(Name name, IReadOnlyCollection<(Article, Scalar)> kitComponents, ArticleTrackingMode trackingMode = ArticleTrackingMode.Entity)
    {
        var articleKit = new Article(kitComponents)
        {
            Name = name,
            TrackingMode = trackingMode
        };

        return articleKit;
    }

    /// <summary>
    /// Without allowed articles the container accepts any article; a <see langword="null"/>
    /// maximum means no limit.
    /// </summary>
    public static Article NewArticleContainer(
        Name name,
        Mass? maxMass = null,
        IReadOnlyCollection<(Article Article, Scalar? MaxQuantity)>? allowedArticles = null)
    {
        var articleContainer = new Article(isContainer: true)
        {
            Name = name,
        };

        articleContainer.Container!.SetMaxMass(maxMass);

        if (allowedArticles is not null)
        {
            foreach (var (article, maxQuantity) in allowedArticles)
            {
                articleContainer.Container!.AddAllowedArticle(article, maxQuantity);
            }
        }

        return articleContainer;
    }

    public void SetMetrics(Mass? mass, Length? lengthX, Length? lengthY, Length? lengthZ)
    {
        Mass = mass;
        Dimension.SetDimensions(lengthX, lengthY, lengthZ);
    }

    /// <summary>
    /// Does nothing when the article already belongs to the partition.
    /// </summary>
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
    /// Does nothing when the article does not belong to the partition.
    /// </summary>
    /// <exception cref="FargoCoreException">The partition is the global partition, which every article must keep.</exception>
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
}
