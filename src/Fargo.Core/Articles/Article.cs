using Fargo.Core.Barcodes;
using Fargo.Core.Partitions;
using System.Drawing;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Core.Articles;

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
public class Article : Entity, IPartitionedEntity, IActivableEntity, IModifiedEntity, IModifiedEntityTypes<ArticleModifiedType>
{
    public static Article CreateArticle(Name name)
        => new(name);

    public static Article CreateArticle(Guid guid, Name name)
        => new(name)
        {
            Guid = guid
        };

    public static Article CreateArticleVariation(Name name, Article fromArticle)
        => new(name, new ArticleVariation(fromArticle));

    public static Article CreateArticlePack(Name name, Article fromArticle, Scalar quantity)
        => new(name, new ArticlePack(fromArticle, quantity));

    public static Article CreateArticleKit(Name name, IReadOnlyCollection<ArticleKitComponent> components)
        => new(name, new ArticleKit(components));

    public static Article CreateArticleContainer(Name name)
        => new(name, new ArticleContainer(null));

    private Article()
    {
    }

    private Article(Name name)
    {
        Name = name;
    }

    /// <summary>
    /// Initialize a new article entity that is a variation of another article.
    /// </summary>
    /// <param name="variation">The variation relationship associated with the article.</param>
    private Article(Name name, ArticleVariation variation)
        : this(name)
    {
        Variation = variation;
    }

    /// <summary>
    /// Initialize a new article entity that is a pack of another article.
    /// </summary>
    /// <param name="pack">The pack relationship associated with the article.</param>
    private Article(Name name, ArticlePack pack)
        : this(name)
    {
        Pack = pack;
    }

    /// <summary>
    /// Initialize a new article entity that is a kit of other articles.
    /// </summary>
    /// <param name="kit">The kit relationship associated with the article.</param>
    private Article(Name name, ArticleKit kit)
        : this(name)
    {
        Kit = kit;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Article"/> class as a container article.
    /// </summary>
    /// <param name="container">The container constraints associated with the article.</param>
    private Article(Name name, ArticleContainer container)
        : this(name)
    {
        Container = container;
    }

    #region General

    /// <summary>
    /// Gets or sets the name of the article.
    /// </summary>
    /// <remarks>
    /// The name identifies the article in the domain and must satisfy
    /// the validation rules defined by <see cref="Name"/>.
    /// </remarks>
    public Name Name { get; private set; }

    public void Rename(Name name)
    {
        if (Name == name)
        {
            return;
        }

        Name = name;
    }

    /// <summary>
    /// Gets or sets the description of the article.
    /// </summary>
    /// <remarks>
    /// If no description is explicitly provided, the value defaults to
    /// <see cref="Description.Empty"/>.
    /// </remarks>
    public Description Description { get; private set; } = Description.Empty;

    public void ChangeDescription(Description description)
    {
        if (Description == description)
        {
            return;
        }

        Description = description;
    }

    /// <summary>
    /// Gets or sets the shelf life of the article.
    /// When <see langword="null"/>, no shelf life constraint is defined.
    /// Persisted as <c>bigint</c> (ticks) in the database.
    /// </summary>
    public TimeSpan? ShelfLife { get; private set; }

    public void SetShelfLife(TimeSpan? shelfLife)
    {
        if (ShelfLife == shelfLife)
        {
            return;
        }

        ShelfLife = shelfLife;
    }

    /// <summary>
    /// Gets or sets the color of the article.
    /// When <see langword="null"/>, no color constraint is defined.
    /// </summary>
    public Color? Color { get; private set; }

    public void SetColor(Color? color)
    {
        if (Color == color)
        {
            return;
        }

        Color = color;
    }

    #endregion General

    #region Metrics

    /// <summary>
    /// Gets or sets the X dimension of the article.
    /// </summary>
    public Length? LengthX { get; private set; }

    /// <summary>
    /// Gets or sets the Y dimension of the article.
    /// </summary>
    public Length? LengthY { get; private set; }

    /// <summary>
    /// Gets or sets the Z dimension of the article.
    /// </summary>
    public Length? LengthZ { get; private set; }

    /// <summary>
    /// Gets or sets the physical mass of the article.
    /// </summary>
    public Mass? Mass { get; private set; }

    public void SetMetrics(ArticleMetrics metrics)
    {
        if (Nullable.Equals(Mass, metrics.Mass) &&
            Nullable.Equals(LengthX, metrics.LengthX) &&
            Nullable.Equals(LengthY, metrics.LengthY) &&
            Nullable.Equals(LengthZ, metrics.LengthZ))
        {
            return;
        }

        Mass = metrics.Mass;
        LengthX = metrics.LengthX;
        LengthY = metrics.LengthY;
        LengthZ = metrics.LengthZ;
    }

    /// <summary>
    /// Gets the volume of the article.
    /// </summary>
    public Volume? Volume => LengthX * LengthY * LengthZ;

    /// <summary>
    /// Gets the density of the article.
    /// </summary>
    public Density? Density => Mass / Volume;

    #endregion Metrics

    #region Barcode

    /// <summary>
    /// EAN-13 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Ean13? Ean13 { get; private set; }

    internal void SetEan13(Ean13? value) => SetBarcode(value, Ean13, next => Ean13 = next);

    /// <summary>
    /// EAN-8 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Ean8? Ean8 { get; private set; }

    internal void SetEan8(Ean8? value) => SetBarcode(value, Ean8, next => Ean8 = next);

    /// <summary>
    /// UPC-A barcode, or <see langword="null"/> when absent.
    /// </summary>
    public UpcA? UpcA { get; private set; }

    internal void SetUpcA(UpcA? value) => SetBarcode(value, UpcA, next => UpcA = next);

    /// <summary>
    /// UPC-E barcode, or <see langword="null"/> when absent.
    /// </summary>
    public UpcE? UpcE { get; private set; }

    internal void SetUpcE(UpcE? value) => SetBarcode(value, UpcE, next => UpcE = next);

    /// <summary>
    /// Code 128 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Code128? Code128 { get; private set; }

    internal void SetCode128(Code128? value) => SetBarcode(value, Code128, next => Code128 = next);

    /// <summary>
    /// Code 39 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Code39? Code39 { get; private set; }

    internal void SetCode39(Code39? value) => SetBarcode(value, Code39, next => Code39 = next);

    /// <summary>
    /// ITF-14 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Itf14? Itf14 { get; private set; }

    internal void SetItf14(Itf14? value) => SetBarcode(value, Itf14, next => Itf14 = next);

    /// <summary>
    /// GS1-128 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Gs1128? Gs1128 { get; private set; }

    internal void SetGs1128(Gs1128? value) => SetBarcode(value, Gs1128, next => Gs1128 = next);

    /// <summary>
    /// QR Code barcode, or <see langword="null"/> when absent.
    /// </summary>
    public QrCode? QrCode { get; private set; }

    internal void SetQrCode(QrCode? value) => SetBarcode(value, QrCode, next => QrCode = next);

    /// <summary>
    /// Data Matrix barcode, or <see langword="null"/> when absent.
    /// </summary>
    public DataMatrix? DataMatrix { get; private set; }

    internal void SetDataMatrix(DataMatrix? value) => SetBarcode(value, DataMatrix, next => DataMatrix = next);

    private static void SetBarcode<TBarcode>(
        TBarcode? value,
        TBarcode? current,
        Action<TBarcode?> setter)
        where TBarcode : struct, IEquatable<TBarcode>
    {
        if (EqualityComparer<TBarcode?>.Default.Equals(current, value))
        {
            return;
        }

        setter(value);
    }

    #endregion Barcode

    #region Variation

    /// <summary>
    /// Gets the variation info associated with the article.
    /// When <see langword="null"/>, no variation constraint is defined.
    /// </summary>
    public ArticleVariation? Variation { get; private init; }

    /// <summary>
    /// Gets a value indicating whether this article is a variation of another article.
    /// </summary>
    public bool IsVariation => Variation is not null;

    #endregion Variation

    #region Pack

    /// <summary>
    /// Gets the pack info associated with the article.
    /// When <see langword="null"/>, no pack constraint is defined.
    /// </summary>
    public ArticlePack? Pack { get; private init; }

    public void SetPackQuantity(Scalar quantity)
    {
        if (Pack is null)
        {
            throw new InvalidOperationException("A pack quantity cannot be set when the article is not a pack.");
        }

        if (Pack.Quantity.Equals(quantity, 0.Amount()))
        {
            return;
        }

        Pack.SetQuantity(quantity);
    }

    /// <summary>
    /// Gets a value indicating whether this article represents a pack.
    /// </summary>
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
    public bool IsKit => Kit is not null;

    #endregion Kit

    #region Container

    /// <summary>
    /// Gets the container constraints associated with the article.
    /// When <see langword="null"/>, the article is not a container.
    /// </summary>
    public ArticleContainer? Container { get; private set; }

    public void SetContainerMaxMass(Mass? maxMass)
    {
        if (Container is null && maxMass is null)
        {
            return;
        }

        if (Container is null)
        {
            Container = new ArticleContainer(maxMass);
            return;
        }

        if (Nullable.Equals(Container.MaxMass, maxMass))
        {
            return;
        }

        Container.SetMaxMass(maxMass);
    }

    /// <summary>
    /// Gets a value indicating whether this article represents a container.
    /// </summary>
    public bool IsContainer => Container is not null;

    #endregion Container

    #region Active

    /// <summary>
    /// Gets a value indicating whether the article is active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Activates the article.
    /// </summary>
    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
    }

    /// <summary>
    /// Deactivates the article.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
    }

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

    public void AddPartition(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (Partitions.Any(p => p.Guid == partition.Guid))
        {
            return;
        }

        Partitions.Add(partition);
    }

    public void RemovePartition(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (!Partitions.Any(p => p.Guid == partition.Guid))
        {
            return;
        }

        Partitions.Remove(partition);
    }

    /// <inheritdoc />
    IReadOnlyCollection<IPartitionEntity> IPartitionedEntity.Partitions => Partitions;

    #endregion Partition

    #region Modified

    public Guid? EditedByGuid { get; private set; }

    public void MarkAsEditedBy(Guid actorGuid)
    {
        EditedByGuid = actorGuid;
    }

    public ArticleModifiedType ModificationTypes { get; private set; }

    public IReadOnlySet<ArticleModifiedType> GetModificationTypes()
    {
        HashSet<ArticleModifiedType> result = [];

        foreach (ArticleModifiedType value in Enum.GetValues<ArticleModifiedType>())
        {
            if (value == ArticleModifiedType.None)
            {
                continue;
            }

            if ((ModificationTypes & value) != 0)
            {
                result.Add(value);
            }
        }

        return result;
    }

    public bool IsEditStarted { get; private set; }

    public void MarkModificationType(ArticleModifiedType modificationType)
    {
        if (!IsEditStarted)
        {
            ModificationTypes = ArticleModifiedType.None;
            IsEditStarted = true;
        }

        ModificationTypes |= modificationType;
    }

    #endregion Modified
}

#region Container

/// <summary>
/// Defines container constraints for an <see cref="Article"/>.
/// </summary>
/// <remarks>
/// A container article represents an article that may contain other articles,
/// optionally constrained by allowed articles, restricted articles, and maximum mass.
/// </remarks>
public sealed class ArticleContainer
{
    private ArticleContainer()
    {
    }

    public ArticleContainer(Mass? maxMass)
    {
        SetMaxMass(maxMass);
    }

    /// <summary>
    /// Gets or sets the maximum mass allowed inside the container.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, no maximum mass constraint is defined.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is less than or equal to zero.
    /// </exception>
    public Mass? MaxMass { get; private set; }

    public void SetMaxMass(Mass? maxMass)
    {
        if (maxMass is not null && maxMass.Value <= Mass.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxMass),
                maxMass,
                "The maximum mass of a container must be greater than zero.");
        }

        MaxMass = maxMass;
    }
}

#endregion Container

#region Variation

/// <summary>
/// Defines an article variation relationship.
/// </summary>
/// <remarks>
/// A variation article is derived from another article, usually representing
/// a different version, option, or presentation of the original article.
/// </remarks>
public sealed class ArticleVariation
{
    private ArticleVariation()
    {
    }

    public ArticleVariation(Article fromArticle)
    {
        FromArticle = fromArticle;
    }

    /// <summary>
    /// Gets the unique identifier of the source article.
    /// </summary>
    public Guid FromArticleGuid { get; private set; }

    /// <summary>
    /// Gets the article from which this variation originates.
    /// </summary>
    public Article FromArticle
    {
        get;
        private set
        {
            FromArticleGuid = value.Guid;
            field = value;
        }
    } = null!;
}

#endregion Variation

#region Pack

/// <summary>
/// Defines an article pack relationship.
/// </summary>
/// <remarks>
/// A pack article represents a quantity of another article grouped as a single article.
/// For example, a pack may represent twelve units of the same source article.
/// </remarks>
public sealed class ArticlePack
{
    private ArticlePack()
    {
    }

    public ArticlePack(Article fromArticle, Scalar quantity)
    {
        FromArticle = fromArticle;
        SetQuantity(quantity);
    }

    /// <summary>
    /// Gets the unique identifier of the source article contained in the pack.
    /// </summary>
    public Guid FromArticleGuid { get; private set; }

    /// <summary>
    /// Gets the article from which this pack is composed.
    /// </summary>
    public Article FromArticle
    {
        get;
        private set
        {
            FromArticleGuid = value.Guid;
            field = value;
        }
    } = null!;

    /// <summary>
    /// Gets or sets the quantity of the source article represented by the pack.
    /// </summary>
    /// <remarks>
    /// The quantity must be greater than zero.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the quantity is less than or equal to zero.
    /// </exception>
    public Scalar Quantity { get; private set; }

    public void SetQuantity(Scalar quantity)
    {
        if (quantity <= 0.Amount())
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                quantity,
                "The pack quantity must be greater than zero.");
        }

        Quantity = quantity;
    }
}

#endregion Pack

#region Kit

/// <summary>
/// Identifies a source article and quantity requested for a kit article.
/// </summary>
public readonly record struct ArticleKitComponentRequest
{
    public ArticleKitComponentRequest(Guid articleGuid, Scalar quantity)
    {
        if (articleGuid == Guid.Empty)
        {
            throw new ArgumentException("A kit component article guid cannot be empty.", nameof(articleGuid));
        }

        if (quantity <= 0.Amount())
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                quantity,
                "A kit component quantity must be greater than zero.");
        }

        ArticleGuid = articleGuid;
        Quantity = quantity;
    }

    /// <summary>
    /// Gets the unique identifier of the source article included in the kit.
    /// </summary>
    public Guid ArticleGuid { get; }

    /// <summary>
    /// Gets the quantity of the source article included in the kit.
    /// </summary>
    public Scalar Quantity { get; }
}

/// <summary>
/// Defines one component of a kit article.
/// </summary>
public sealed class ArticleKitComponent
{
    private ArticleKitComponent()
    {
    }

    public ArticleKitComponent(Article article, Scalar quantity)
    {
        Article = article;
        SetQuantity(quantity);
    }

    /// <summary>
    /// Gets the unique identifier of the source article included in the kit.
    /// </summary>
    public Guid ArticleGuid { get; private set; }

    /// <summary>
    /// Gets the source article included in the kit.
    /// </summary>
    public Article Article
    {
        get;
        private set
        {
            ArticleGuid = value.Guid;
            field = value;
        }
    } = null!;

    /// <summary>
    /// Gets the quantity of the source article included in the kit.
    /// </summary>
    public Scalar Quantity { get; private set; }

    public void SetQuantity(Scalar quantity)
    {
        if (quantity <= 0.Amount())
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                quantity,
                "A kit component quantity must be greater than zero.");
        }

        Quantity = quantity;
    }
}

/// <summary>
/// Defines an article kit relationship.
/// </summary>
/// <remarks>
/// A kit article is composed of one or more components.
/// Each component defines the source article and the quantity included in the kit.
/// </remarks>
public sealed class ArticleKit
{
    private ArticleKit()
    {
    }

    public ArticleKit(IReadOnlyCollection<ArticleKitComponent> components)
    {
        SetComponents(components);
    }

    /// <summary>
    /// Gets the components that compose the kit.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the collection contains null components, or contains duplicated source articles.
    /// </exception>
    public IReadOnlyCollection<ArticleKitComponent> Components { get; private set; } = [];

    private void SetComponents(IReadOnlyCollection<ArticleKitComponent> components)
    {
        if (components.Any(component => component is null))
        {
            throw new ArgumentException(
                "A kit cannot contain null components.",
                nameof(components));
        }

        if (components
            .GroupBy(component => component.ArticleGuid)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "A kit cannot contain duplicated source articles.",
                nameof(components));
        }

        Components = components;
    }
}

#endregion Kit
