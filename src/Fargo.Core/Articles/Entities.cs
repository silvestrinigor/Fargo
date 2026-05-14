using Fargo.Core.Barcodes;
using Fargo.Core.Identity;
using Fargo.Core.Partitions;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToScalar;

namespace Fargo.Core.Articles;

[Flags]
public enum ArticleModifiedType
{
    None = 0,
    General = 1 << 0,
    Metrics = 1 << 1,
    Barcode = 1 << 2,
    Partition = 1 << 3,
    Container = 1 << 4,
    Relation = 1 << 5
}

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
public class Article : ModifiedEntity, IPartitionedEntity, IActivable, IModifiedEntityTypes<ArticleModifiedType>
{
    [NotMapped]
    private Actor? editActor;

    /// <summary>
    /// Initialize a new article entity.
    /// </summary>
    public Article()
    {
    }

    public Article(Name name, Actor actor)
    {
        StartEdit(actor);
        ValidateAction(ActionType.CreateArticle);
        Rename(name);
    }

    /// <summary>
    /// Initialize a new article entity that is a variation of another article.
    /// </summary>
    /// <param name="variation">The variation relationship associated with the article.</param>
    public Article(Name name, ArticleVariation variation, Actor actor)
        : this(name, actor)
    {
        Variation = variation;
        MarkModified(ArticleModifiedType.Relation);
    }

    /// <summary>
    /// Initialize a new article entity that is a pack of another article.
    /// </summary>
    /// <param name="pack">The pack relationship associated with the article.</param>
    public Article(Name name, ArticlePack pack, Actor actor)
        : this(name, actor)
    {
        Pack = pack;
        MarkModified(ArticleModifiedType.Relation);
    }

    /// <summary>
    /// Initialize a new article entity that is a kit of other articles.
    /// </summary>
    /// <param name="kit">The kit relationship associated with the article.</param>
    public Article(Name name, ArticleKit kit, Actor actor)
        : this(name, actor)
    {
        Kit = kit;
        MarkModified(ArticleModifiedType.Relation);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Article"/> class as a container article.
    /// </summary>
    /// <param name="container">The container constraints associated with the article.</param>
    public Article(Name name, ArticleContainer container, Actor actor)
        : this(name, actor)
    {
        Container = container;
        MarkModified(ArticleModifiedType.Container);
    }

    /// <summary>
    /// Gets or sets the name of the article.
    /// </summary>
    /// <remarks>
    /// The name identifies the article in the domain and must satisfy
    /// the validation rules defined by <see cref="Name"/>.
    /// </remarks>
    public Name Name { get; private set; }

    /// <summary>
    /// Gets or sets the description of the article.
    /// </summary>
    /// <remarks>
    /// If no description is explicitly provided, the value defaults to
    /// <see cref="Description.Empty"/>.
    /// </remarks>
    public Description Description { get; private set; } = Description.Empty;

    /// <summary>
    /// Gets or sets the shelf life of the article.
    /// When <see langword="null"/>, no shelf life constraint is defined.
    /// Persisted as <c>bigint</c> (ticks) in the database.
    /// </summary>
    public TimeSpan? ShelfLife { get; private set; }

    /// <summary>
    /// Gets or sets the color of the article.
    /// When <see langword="null"/>, no color constraint is defined.
    /// </summary>
    public Color? Color { get; private set; }

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

    /// <summary>
    /// EAN-8 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Ean8? Ean8 { get; private set; }

    /// <summary>
    /// UPC-A barcode, or <see langword="null"/> when absent.
    /// </summary>
    public UpcA? UpcA { get; private set; }

    /// <summary>
    /// UPC-E barcode, or <see langword="null"/> when absent.
    /// </summary>
    public UpcE? UpcE { get; private set; }

    /// <summary>
    /// Code 128 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Code128? Code128 { get; private set; }

    /// <summary>
    /// Code 39 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Code39? Code39 { get; private set; }

    /// <summary>
    /// ITF-14 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Itf14? Itf14 { get; private set; }

    /// <summary>
    /// GS1-128 barcode, or <see langword="null"/> when absent.
    /// </summary>
    public Gs1128? Gs1128 { get; private set; }

    /// <summary>
    /// QR Code barcode, or <see langword="null"/> when absent.
    /// </summary>
    public QrCode? QrCode { get; private set; }

    /// <summary>
    /// Data Matrix barcode, or <see langword="null"/> when absent.
    /// </summary>
    public DataMatrix? DataMatrix { get; private set; }

    #endregion Barcode

    #region Relation

    /// <summary>
    /// Gets the variation info associated with the article.
    /// When <see langword="null"/>, no variation constraint is defined.
    /// </summary>
    public ArticleVariation? Variation { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this article is a variation of another article.
    /// </summary>
    public bool IsVariation => Variation is not null;

    /// <summary>
    /// Gets the pack info associated with the article.
    /// When <see langword="null"/>, no pack constraint is defined.
    /// </summary>
    public ArticlePack? Pack { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this article represents a pack.
    /// </summary>
    public bool IsPack => Pack is not null;

    /// <summary>
    /// Gets the kit info associated with the article.
    /// When <see langword="null"/>, no kit constraint is defined.
    /// </summary>
    public ArticleKit? Kit { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this article represents a kit.
    /// </summary>
    public bool IsKit => Kit is not null;

    #endregion Relation

    #region Container

    /// <summary>
    /// Gets the container constraints associated with the article.
    /// When <see langword="null"/>, the article is not a container.
    /// </summary>
    public ArticleContainer? Container { get; private set; }

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
        MarkModified(ArticleModifiedType.General);
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
        MarkModified(ArticleModifiedType.General);
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

    /// <inheritdoc />
    IReadOnlyCollection<IPartitionEntity> IPartitionedEntity.Partitions => Partitions;

    #endregion Partition

    #region Modified

    public void StartEdit(Actor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);

        editActor = actor;
        ModificationTypes = ArticleModifiedType.None;
    }

    public void Rename(Name name)
    {
        if (Name == name)
        {
            return;
        }

        Name = name;
        MarkModified(ArticleModifiedType.General);
    }

    public void ChangeDescription(Description description)
    {
        if (Description == description)
        {
            return;
        }

        Description = description;
        MarkModified(ArticleModifiedType.General);
    }

    public void SetShelfLife(TimeSpan? shelfLife)
    {
        if (ShelfLife == shelfLife)
        {
            return;
        }

        ShelfLife = shelfLife;
        MarkModified(ArticleModifiedType.General);
    }

    public void SetColor(Color? color)
    {
        if (Color == color)
        {
            return;
        }

        Color = color;
        MarkModified(ArticleModifiedType.General);
    }

    public void SetMetrics(Mass? mass, Length? lengthX, Length? lengthY, Length? lengthZ)
    {
        if (Nullable.Equals(Mass, mass) &&
            Nullable.Equals(LengthX, lengthX) &&
            Nullable.Equals(LengthY, lengthY) &&
            Nullable.Equals(LengthZ, lengthZ))
        {
            return;
        }

        Mass = mass;
        LengthX = lengthX;
        LengthY = lengthY;
        LengthZ = lengthZ;
        MarkModified(ArticleModifiedType.Metrics);
    }

    internal void SetEan13(Ean13? value, Actor actor) => SetBarcode(value, Ean13, next => Ean13 = next, actor);

    internal void SetEan8(Ean8? value, Actor actor) => SetBarcode(value, Ean8, next => Ean8 = next, actor);

    internal void SetUpcA(UpcA? value, Actor actor) => SetBarcode(value, UpcA, next => UpcA = next, actor);

    internal void SetUpcE(UpcE? value, Actor actor) => SetBarcode(value, UpcE, next => UpcE = next, actor);

    internal void SetCode128(Code128? value, Actor actor) => SetBarcode(value, Code128, next => Code128 = next, actor);

    internal void SetCode39(Code39? value, Actor actor) => SetBarcode(value, Code39, next => Code39 = next, actor);

    internal void SetItf14(Itf14? value, Actor actor) => SetBarcode(value, Itf14, next => Itf14 = next, actor);

    internal void SetGs1128(Gs1128? value, Actor actor) => SetBarcode(value, Gs1128, next => Gs1128 = next, actor);

    internal void SetQrCode(QrCode? value, Actor actor) => SetBarcode(value, QrCode, next => QrCode = next, actor);

    internal void SetDataMatrix(DataMatrix? value, Actor actor) => SetBarcode(value, DataMatrix, next => DataMatrix = next, actor);

    public void AddPartition(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (Partitions.Any(p => p.Guid == partition.Guid))
        {
            return;
        }

        ValidatePartitionAccess(partition.Guid);
        Partitions.Add(partition);
        MarkModified(ArticleModifiedType.Partition);
    }

    public void RemovePartition(Partition partition)
    {
        ArgumentNullException.ThrowIfNull(partition);

        if (!Partitions.Any(p => p.Guid == partition.Guid))
        {
            return;
        }

        ValidatePartitionAccess(partition.Guid);
        Partitions.Remove(partition);
        MarkModified(ArticleModifiedType.Partition);
    }

    public void SetContainerMaxMass(Mass? maxMass)
    {
        if (Container is null && maxMass is null)
        {
            return;
        }

        if (Container is null)
        {
            Container = new ArticleContainer(maxMass);
            MarkModified(ArticleModifiedType.Container);
            return;
        }

        if (Nullable.Equals(Container.MaxMass, maxMass))
        {
            return;
        }

        Container.SetMaxMass(maxMass);
        MarkModified(ArticleModifiedType.Container);
    }

    public void SetVariationFromArticle(Article fromArticle)
    {
        ArgumentNullException.ThrowIfNull(fromArticle);

        if (Variation?.FromArticleGuid == fromArticle.Guid)
        {
            return;
        }

        Variation = new ArticleVariation(fromArticle);
        MarkModified(ArticleModifiedType.Relation);
    }

    public void SetPackFromArticle(Article fromArticle)
    {
        ArgumentNullException.ThrowIfNull(fromArticle);

        if (Pack?.FromArticleGuid == fromArticle.Guid)
        {
            return;
        }

        Pack = new ArticlePack(fromArticle, Pack?.Quantity ?? 1.Amount());
        MarkModified(ArticleModifiedType.Relation);
    }

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
        MarkModified(ArticleModifiedType.Relation);
    }

    public void SetKitArticles(IReadOnlyCollection<ArticlePack> packs)
    {
        ArgumentNullException.ThrowIfNull(packs);

        Kit = new ArticleKit(packs);
        MarkModified(ArticleModifiedType.Relation);
    }

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

    public ArticleModifiedType ModificationTypes { get; private set; }

    private void SetBarcode<TBarcode>(
        TBarcode? value,
        TBarcode? current,
        Action<TBarcode?> setter,
        Actor actor)
        where TBarcode : struct, IEquatable<TBarcode>
    {
        ArgumentNullException.ThrowIfNull(actor);

        if (EqualityComparer<TBarcode?>.Default.Equals(current, value))
        {
            return;
        }

        if (current is null && value is not null)
        {
            ValidateAction(actor, ActionType.AddBarcode);
        }
        else if (current is not null && value is null)
        {
            ValidateAction(actor, ActionType.RemoveBarcode);
        }
        else
        {
            ValidateAction(actor, ActionType.AddBarcode);
            ValidateAction(actor, ActionType.RemoveBarcode);
        }

        setter(value);
        MarkModified(ArticleModifiedType.Barcode, actor);
    }

    private void MarkModified(ArticleModifiedType type)
    {
        var actor = GetEditActor();

        MarkModified(type, actor);
    }

    private void MarkModified(ArticleModifiedType type, Actor actor)
    {
        ValidateAction(actor, ActionType.EditArticle);

        if (Partitions.Count > 0 && !actor.HasAccess(this))
        {
            throw new ArticleAccessDeniedFargoDomainException(Guid, actor.Guid);
        }

        MarkAsEdited(actor.Guid);
        ModificationTypes |= type;
    }

    private Actor GetEditActor()
        => editActor ?? throw new ArticleEditNotStartedFargoDomainException(Guid);

    private void ValidateAction(ActionType action)
    {
        var actor = GetEditActor();

        ValidateAction(actor, action);
    }

    private void ValidateAction(Actor actor, ActionType action)
    {
        if (!actor.HasActionPermission(action))
        {
            throw new ArticleActionNotAuthorizedFargoDomainException(Guid, actor.Guid, action);
        }
    }

    private void ValidatePartitionAccess(Guid partitionGuid)
    {
        var actor = GetEditActor();

        if (!actor.HasPartitionAccess(partitionGuid))
        {
            throw new ArticlePartitionAccessDeniedFargoDomainException(Guid, partitionGuid, actor.Guid);
        }
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
/// Defines an article kit relationship.
/// </summary>
/// <remarks>
/// A kit article is composed of one or more article packs.
/// Each pack defines the source article and the quantity included in the kit.
/// </remarks>
public sealed class ArticleKit
{
    private ArticleKit()
    {
    }

    public ArticleKit(IReadOnlyCollection<ArticlePack> fromArticles)
    {
        SetFromArticles(fromArticles);
    }

    /// <summary>
    /// Gets the article packs that compose the kit.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the collection contains null packs, or contains duplicated source articles.
    /// </exception>
    public IReadOnlyCollection<ArticlePack> FromArticles { get; private set; } = [];

    private void SetFromArticles(IReadOnlyCollection<ArticlePack> fromArticles)
    {
        if (fromArticles.Any(articlePack => articlePack is null))
        {
            throw new ArgumentException(
                "A kit cannot contain null article packs.",
                nameof(fromArticles));
        }

        if (fromArticles
            .GroupBy(articlePack => articlePack.FromArticleGuid)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "A kit cannot contain duplicated source articles.",
                nameof(fromArticles));
        }

        FromArticles = fromArticles;
    }
}

#endregion Kit
